namespace PointW.WebApi.ResourceModel.TestResources
{
    public class ResourceWithEmbeddedProducts : Resource
    {
        public string Name { get; set; }
        public ProductResource Car { get; set; }
        public ProductResource Computer { get; set; }
    }
}
