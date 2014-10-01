namespace PointW.WebApi.ResourceModel
{
    public class Resource : IResource
    {
        public LinkCollection Relations { get; set; }

        public Resource()
        {
            Relations = new LinkCollection();
        }
    }
}
