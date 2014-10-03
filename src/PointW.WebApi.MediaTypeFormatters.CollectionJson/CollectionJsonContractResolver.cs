using System;
using System.Collections;
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

                var collection = new JsonProperty
                {
                    DeclaringType = type,
                    PropertyType = typeof(Dictionary<string, object>),
                    PropertyName = "collection",
                    ValueProvider = new ResourceValueProvider(),
                    Readable = true,
                    Writable = false //,
                    // ShouldSerialize = instance => instance.GetType().GetProperties().Any(p => IsInstanceEmbeddable(p, instance))
                };


                properties.Add(collection);
            }

            return properties;
        }
    }



    internal class ResourceValueProvider : IValueProvider
    {
        public void SetValue(object target, object value)
        {
            throw new NotImplementedException();
        }

        public object GetValue(object target)
        {
            var links = target.GetType()
                .GetProperties().Where(p => p.Name == "Relations");

            var props = target.GetType()
                .GetProperties().Where(p => p.Name != "Relations");

            var rtn = links.ToDictionary(p => (p.Name == "Relations" ? "links" : p.Name), p => p.GetValue(target));

            rtn.Add("items", new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"data", props.ToDictionary(p => p.Name, p => p.GetValue(target))}
                }
            });
                //props.ToDictionary(p => p.Name, p => p.GetValue(target)));

            return rtn;
        }
    }

}