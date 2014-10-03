namespace PointW.WebApi.ResourceModel.TestResources
{
    public class ResourceWithHiddenId : Resource
    {
        [NeverShow]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
