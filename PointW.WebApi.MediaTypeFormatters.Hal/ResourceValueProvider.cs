using System;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace PointW.WebApi.MediaTypeFormatters.Hal
{
    internal class ResourceValueProvider : IValueProvider
    {
        public void SetValue(object target, object value)
        {
            throw new NotImplementedException();
        }

        public object GetValue(object target)
        {
            var embeddedResources = target.GetType()
                .GetProperties()
                .Where(p => HalContractResolver.IsInstanceEmbeddable(p, target));

            var rtn = embeddedResources.ToDictionary(p => p.Name, p => p.GetValue(target));

            return rtn;
        }
    }
}