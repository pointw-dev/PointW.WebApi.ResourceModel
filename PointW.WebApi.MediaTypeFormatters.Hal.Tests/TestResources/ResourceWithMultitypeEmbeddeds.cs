using PointW.WebApi.ResourceModel;

namespace PointW.WebApi.MediaTypeFormatters.Hal.Tests.TestResources
{
    public class ResourceWithMultitypeEmbeddeds : Resource
    {
        public string Name { get; set; }
        public BasicResource Employee { get; set; }
        public ProductResource Car { get; set; }
    }
}