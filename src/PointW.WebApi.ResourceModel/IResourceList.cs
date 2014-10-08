using System.Collections.Generic;

namespace PointW.WebApi.ResourceModel
{
    public interface IResourceList<TResource> : IResource where TResource : IResource
    {
        ICollection<TResource> Items { get; set; }
    }

    // public interface IResourceList : IResource
    // {
    //     ICollection<IResource> Items { get; set; } 
    // }
}
