using PointW.WebApi.ResourceModel;

namespace PointW.WebApi.MediaTypeFormatters.Hal.Tests.TestResources
{
    public class ResourceWithHiddenId : Resource
    {
        [NeverShow]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
