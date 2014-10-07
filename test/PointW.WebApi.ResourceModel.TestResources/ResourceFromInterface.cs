﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointW.WebApi.ResourceModel.TestResources
{
    public class ResourceFromInterface : IResource
    {
        public ResourceFromInterface()
        {
            Relations = new LinkCollection();
        }

        public ResourceFromInterface(LinkCollection relations)
        {
            Relations = relations;
        }

        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        public LinkCollection Relations { get; set; }
    }
}
