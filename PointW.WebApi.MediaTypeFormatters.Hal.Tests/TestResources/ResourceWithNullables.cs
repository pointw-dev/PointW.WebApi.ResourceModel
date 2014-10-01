using PointW.WebApi.ResourceModel;

namespace PointW.WebApi.MediaTypeFormatters.Hal.Tests.TestResources
{
    public class ResourceWithNullables : Resource
    {
        public string Name { get; set; }
        public int? Number { get; set; }
    }
}
