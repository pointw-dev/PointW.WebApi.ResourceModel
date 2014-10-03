namespace PointW.WebApi.ResourceModel.TestResources
{
    public class RecursiveResource : Resource
    {
        public string Name { get; set; }
        public RecursiveResource NestedResource { get; set; }
    }
}
