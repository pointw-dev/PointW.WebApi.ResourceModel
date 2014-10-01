using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PointW.WebApi.ResourceModel;

namespace PointW.WebApi.MediaTypeFormatters.Hal.Tests.TestResources
{
    public class ResourceWithEmbeddedProducts : Resource
    {
        public string Name { get; set; }
        public ProductResource Car { get; set; }
        public ProductResource Computer { get; set; }
    }
}
