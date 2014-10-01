using PointW.WebApi.ResourceModel;

namespace PointW.WebApi.MediaTypeFormatters.Hal.Tests.TestResources
{
    public class BasicResource : Resource
    {
        public string Name { get; set; }

        // [AlternateRepresentation(MediaType = "image/jpeg")]
        // public string ImageRepresentation { get; set; }

    }
}
