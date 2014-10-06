using System.Collections.Generic;

namespace PointW.WebApi.ResourceModel
{
    public interface IResourceList : IResource
    {
        IList<Resource> Items { get; set; }
    }
}
