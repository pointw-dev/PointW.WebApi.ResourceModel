using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PointW.WebApi.ResourceModel
{
    public class SimpleResourceList<TResource> : IResourceList<TResource> where TResource : IResource
    {
        public SimpleResourceList() 
        {
            Items = new Collection<TResource>();
            Relations = new LinkCollection();
        }

        public LinkCollection Relations { get; set; }
        public ICollection<TResource> Items { get; set; }

        public int Count { get { return Items.Count; } }
    }



    public class SimpleResourceList : IResourceList<IResource>
    {
        public SimpleResourceList()
        {
            Items = new Collection<IResource>();
            Relations = new LinkCollection();
        }

        public ICollection<IResource> Items { get; set; }
        public LinkCollection Relations { get; set; }

        public int Count { get { return Items.Count; } }
    }
}
