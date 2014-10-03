using System.Collections.Generic;

namespace PointW.WebApi.ResourceModel.TestResources
{
    public class ResourceWithEmbeddedLists : Resource
    {
        public string Name { get; set; }
        public List<ProductResource> Cars { get; set; }
        public List<ProductResource> Computers { get; set; }
    }
}
