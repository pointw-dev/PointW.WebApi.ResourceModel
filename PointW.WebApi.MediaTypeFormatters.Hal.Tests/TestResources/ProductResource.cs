using PointW.WebApi.ResourceModel;

namespace PointW.WebApi.MediaTypeFormatters.Hal.Tests.TestResources
{
    public class ProductResource : Resource
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
    }
}