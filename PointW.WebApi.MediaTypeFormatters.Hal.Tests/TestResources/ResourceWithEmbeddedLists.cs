using System.Collections.Generic;
using PointW.WebApi.ResourceModel;

namespace PointW.WebApi.MediaTypeFormatters.Hal.Tests.TestResources
{
    public class ResourceWithEmbeddedLists : Resource
    {
        public string Name { get; set; }
        public List<ProductResource> Cars { get; set; }
        public List<ProductResource> Computers { get; set; }
    }
}
