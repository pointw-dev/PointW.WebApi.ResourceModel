using PointW.WebApi.ResourceModel;

namespace PointW.WebApi.MediaTypeFormatters.Hal.Tests.TestResources
{
    public class ResourceWithAlwaysShowProperty : Resource
    {
        public string Name { get; set; }
        
        [AlwaysShow]
        public int? Number { get; set; }
    }
}
