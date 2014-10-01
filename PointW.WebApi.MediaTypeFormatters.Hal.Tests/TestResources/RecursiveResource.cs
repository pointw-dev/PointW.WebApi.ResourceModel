using PointW.WebApi.ResourceModel;

namespace PointW.WebApi.MediaTypeFormatters.Hal.Tests.TestResources
{
    public class RecursiveResource : Resource
    {
        public string Name { get; set; }
        public RecursiveResource NestedResource { get; set; }
    }
}
