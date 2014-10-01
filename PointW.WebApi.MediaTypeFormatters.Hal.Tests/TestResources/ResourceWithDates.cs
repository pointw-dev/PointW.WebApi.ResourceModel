using System;
using PointW.WebApi.ResourceModel;

namespace PointW.WebApi.MediaTypeFormatters.Hal.Tests.TestResources
{
    public class ResourceWithDates : Resource
    {
        public string Name { get; set; }
        
        [ShowDateOnly]
        public DateTime? DateOfHire { get; set; }

        [ShowTimeOnly]
        public DateTime? StartTime { get; set; }

        public DateTime? FirstShift { get; set; }
    }

}