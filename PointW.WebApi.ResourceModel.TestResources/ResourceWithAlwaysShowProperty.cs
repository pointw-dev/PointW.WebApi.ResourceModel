namespace PointW.WebApi.ResourceModel.TestResources
{
    public class ResourceWithAlwaysShowProperty : Resource
    {
        public string Name { get; set; }
        
        [AlwaysShow]
        public int? Number { get; set; }
    }
}
