using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PointW.WebApi.ResourceModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PointW.WebApi.MediaTypeFormatters.CollectionJson
{
    internal class CollectionJsonContractResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            // TODO: this came over from HAL, but doesn't work - overwritten by resourcevalueprovider.  RECTIFY!
            // member.DeclaringType.Name == "Resource"
            if (member.DeclaringType != null && (typeof(IResource).IsAssignableFrom(member.DeclaringType) && property.PropertyName.Equals("relations", StringComparison.OrdinalIgnoreCase)))
            {
                property.PropertyName = "links";
            }

            var hasIgnore = member.GetCustomAttributes(typeof(NeverShowAttribute), true);

            if (hasIgnore.Any())
            {
                property.ShouldSerialize = instance => false;
            }
            else
            {
                var hasAlways = member.GetCustomAttributes(typeof(AlwaysShowAttribute), true);
                if (hasAlways.Any())
                {
                    property.NullValueHandling = NullValueHandling.Include;
                }
            }

            return property;
        }



        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);

            if (typeof(IResource).IsAssignableFrom(type))
            {
                properties = properties.Where((p => false)).ToList();

                // var vp = typeof(IResourceList).IsAssignableFrom(type) // (type.GetInterface("IResourceList`1") != null)
                var vp = (type.GetInterface("IResourceList`1") != null)
                    ? (IValueProvider)new ResourceListValueProvider()
                    : new ResourceValueProvider();

                var collection = new JsonProperty
                {
                    DeclaringType = type,
                    PropertyType = typeof(Dictionary<string, object>),
                    PropertyName = "collection",
                    ValueProvider = vp,
                    Readable = true,
                    Writable = false //,
                    // ShouldSerialize = instance => instance.GetType().GetProperties().Any(p => IsInstanceEmbeddable(p, instance))
                };


                properties.Add(collection);
            }

            return properties;
        }
    }
}