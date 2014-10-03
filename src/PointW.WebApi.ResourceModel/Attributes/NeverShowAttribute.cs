using System;

namespace PointW.WebApi.ResourceModel
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NeverShowAttribute : Attribute
    {
    }
}