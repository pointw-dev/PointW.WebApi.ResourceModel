using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PointW.WebApi.ResourceModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PointW.WebApi.MediaTypeFormatters.Hal
{
    internal class HalContractResolver : CamelCasePropertyNamesContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            // member.DeclaringType.Name == "Resource"
            if (member.DeclaringType != null && (typeof(IResource).IsAssignableFrom(member.DeclaringType) && property.PropertyName.Equals("relations", StringComparison.OrdinalIgnoreCase)))
            {
                property.PropertyName = "_links";
            }

            var hasIgnore = member.GetCustomAttributes(typeof(NeverShowAttribute), true);

            if (hasIgnore.Any())
            {
                property.ShouldSerialize = instance => false;
            }
            else
            {
                var hasAlways = member.GetCustomAttributes(typeof (AlwaysShowAttribute), true);
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

            if (typeof (IResource).IsAssignableFrom(type) &&
                properties.Any(p => IsTypeEmbeddable(p.PropertyType)))
            {
                properties = properties.Where((p => !IsTypeEmbeddable(p.PropertyType))).ToList();

                var embedded = new JsonProperty
                {
                    DeclaringType = type,
                    PropertyType = typeof(Dictionary<string, object>),
                    PropertyName = "_embedded",
                    ValueProvider = new ResourceValueProvider(),
                    Readable = true,
                    Writable = false,
                    ShouldSerialize = instance => instance.GetType().GetProperties().Any(p => IsInstanceEmbeddable(p, instance))
                };

                properties.Add(embedded);
            }

            return properties;
        }



        public static bool IsInstanceEmbeddable(PropertyInfo p, object instance)
        {
            var embeddable = IsTypeEmbeddable(p.PropertyType);

            if (!embeddable) return false;
            
            var val = p.GetValue(instance);

            if (val != null)
            {
                if (typeof (ICollection).IsAssignableFrom(p.PropertyType))
                {
                    embeddable = ((ICollection) val).Cast<object>().Any(i => i is IResource);
                }
            }
            else
            {
                embeddable = p.GetCustomAttributes(typeof (AlwaysShowAttribute), true).Any();
            }

            return embeddable;
        }



        private static bool IsTypeEmbeddable(Type type)
        {
            var isResource = typeof (IResource).IsAssignableFrom(type);
            var isCollection = type.Name == "ICollection`1" && typeof (IResource).IsAssignableFrom(type.GetGenericArguments()[0]) ||
                (typeof (ICollection).IsAssignableFrom(type) && !typeof(LinkCollection).IsAssignableFrom(type));

            return isResource || isCollection;
        }
    }
}