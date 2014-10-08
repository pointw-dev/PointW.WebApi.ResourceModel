using System.Collections.Generic;

namespace PointW.WebApi.ResourceModel
{
    // public class SimpleResourceList2<TResource> : IResourceList<TResource> where TResource : IResource
    // {
    //     public ICollection<TResource> Items { get; set; }
    //     public LinkCollection Relations { get; set; }
    // }

    public class SimpleResourceList2 : IResourceList
    {
        public ICollection<IResource> Items { get; set; }
        public LinkCollection Relations { get; set; }
    }
}
