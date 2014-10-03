using System.Collections.Generic;

namespace PointW.WebApi.ResourceModel
{
    public class SimpleResourceList<TResource> : Resource where TResource : Resource
    {
        public SimpleResourceList() 
        {
            Items = new List<TResource>();
        }
        public List<TResource> Items { get; set; }
        public int Count { get { return Items.Count; } }
    }
}
