using System;

namespace PointW.WebApi.ResourceModel
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AlternateRepresentationAttribute : Attribute // aka StaticRepresentation?
    {
        public string MediaType { get; set; }
        public string Profile { get; set; }
        public string Version { get; set; }
        // alternate approaches:
        // - attach attrib to a method to allow greater flexibility (not really get; is a method - should not presume params of formatter as caller)
        // - Resource class has Dictionary<string, Stream> where string is the media type and stream provides it
        // - forget it - insist on special formatters?
    }
}