using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointW.WebApi.ResourceModel.TestResources
{
    public class TypedResourceList<TResource> : IResourceList where TResource : Resource
    {
        private List<TResource> _items;

        public TypedResourceList(List<TResource> items)
        {
            _items = items;
        }

        public LinkCollection Relations { get; set; }
        public IList<Resource> Items { 
            get { return (IList<Resource>) _items; }
            set { _items = (List<TResource>) value;}
        }
    }
}
