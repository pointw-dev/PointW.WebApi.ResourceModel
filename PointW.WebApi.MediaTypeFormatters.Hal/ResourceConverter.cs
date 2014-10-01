using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using PointW.WebApi.ResourceModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PointW.WebApi.MediaTypeFormatters.Hal
{
    internal class ResourceConverter : JsonConverter
    {
        const string StreamingContextResourceConverterToken = "hal+json";
        const StreamingContextStates StreamingContextResourceConverterState = StreamingContextStates.Other;



        public override bool CanConvert(Type objectType)
        {
            return typeof(IResource).IsAssignableFrom(objectType);
        }

        
        
        public static bool IsResourceConverterContext(StreamingContext context)
        {
            return context.Context is string &&
                   (string)context.Context == StreamingContextResourceConverterToken &&
                   context.State == StreamingContextResourceConverterState;
        }



        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var resource = (IResource)value;

            var saveContext = serializer.Context;
            serializer.Context = GetResourceConverterContext();
            serializer.Converters.Remove(this);
            serializer.Serialize(writer, resource);
            serializer.Converters.Add(this);
            serializer.Context = saveContext;
        }



        private static StreamingContext GetResourceConverterContext()
        {
            return new StreamingContext(StreamingContextResourceConverterState, StreamingContextResourceConverterToken);
        }



        public override object ReadJson(JsonReader reader, Type resourceType, object existingValue, JsonSerializer serializer)
        {
            return ConvertJsonObjectToResource(JObject.Load(reader), resourceType);
        }



        private static object ConvertJsonObjectToResource(JObject jsonObject, Type resourceType)
        {
            // peel out simple properties at this level
            var resource = jsonObject.ToObject(resourceType);

            // get any embedded objects
            var embeddedItems = jsonObject["_embedded"];

            if (embeddedItems != null)
            {
                MatchEmbeddedItemsToResourceProperties(embeddedItems, resource);
            }

            return resource;
        }



        private static void MatchEmbeddedItemsToResourceProperties(JToken embeddedItems, object resource)
        {
            var properties = GetEmbeddableProperties(resource); // i.e. properties of type either IResource or IList<IResource>

            foreach (var property in properties)
            {
                AssignEmbeddedItemsToMatchingProperty(embeddedItems, property, resource);
            }
        }



        private static IEnumerable<PropertyInfo> GetEmbeddableProperties(object resource)
        {
            return resource.GetType()
                    .GetProperties()
                    .Where(
                        p =>
                            typeof(IResource).IsAssignableFrom(p.PropertyType) ||
                            (typeof(IList).IsAssignableFrom(p.PropertyType)));
        }



        private static void AssignEmbeddedItemsToMatchingProperty(JToken embeddedItems, PropertyInfo property, object resource)
        {
            var key = ToCamelCase(property.Name);
            var embeddedItem = embeddedItems[key];

            if (embeddedItem != null)
            {
                if (typeof (IResource).IsAssignableFrom(property.PropertyType)) // this embedded item is directly assigned to a property
                {
                    SetPropertyToEmbeddedResource(embeddedItem, property, resource);
                }
                else // this embedded item is an array to be assigned to a List property
                {
                    AddArrayItemsToListProperty(embeddedItem, property, resource);
                }
            }
        }



        private static void SetPropertyToEmbeddedResource(JToken embeddedJson, PropertyInfo property, object resource)
        {
            var embeddedResource = ConvertJsonObjectToResource(embeddedJson.ToObject<JObject>(), property.PropertyType);
            property.SetValue(resource, embeddedResource);
        }



        private static void AddArrayItemsToListProperty(JToken embeddedJson, PropertyInfo property, object resource)
        {
            var typeOfList = property.PropertyType.GenericTypeArguments[0];

            if (typeof (IResource).IsAssignableFrom(typeOfList)) // this property is an IList, but is it of IResources?
            {
                var list = property.GetValue(resource) as IList;
                if (list != null)
                {
                    foreach (var item in embeddedJson.ToObject<JArray>())
                    {
                        var res = ConvertJsonObjectToResource(item.ToObject<JObject>(), typeOfList);
                        list.Add(res);
                    }
                    property.SetValue(resource, list);
                }
            }
        }



        private static string ToCamelCase(string s) // TODO: this should be elevated to something more universally usable
        {
            if (string.IsNullOrEmpty(s))
                return s;

            if (!char.IsUpper(s[0]))
                return s;

            var chars = s.ToCharArray();

            for (var i = 0; i < chars.Length; i++)
            {
                var hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1]))
                    break;

                chars[i] = char.ToLowerInvariant(chars[i]);
            }

            return new string(chars);
        }
    }
}