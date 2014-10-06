using System.Collections.Generic;

namespace PointW.WebApi.ResourceModel
{
    public class SimpleResourceList2 : IResourceList
    {
        public IList<Resource> Items { get; set; }
        public LinkCollection Relations { get; set; }
    }
}
