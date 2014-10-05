using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PointW.WebApi.ResourceModel;
using PointW.WebApi.ResourceModel.TestResources;

namespace PointW.WebApi.MediaTypeFormatters.CollectionJson.Tests
{
    [TestClass]
    public class Test_CJ02_ResourceCollections
    {
        private CollectionJsonMediaTypeFormatter _formatter;
        private SimpleResourceList<BasicResource> _list;



        [TestInitialize]
        public void Setup()
        {
            _list = new SimpleResourceList<BasicResource>();
            _list.Relations.Add("self", new Link { Href = "selfhref" });

            _list.Items.Add(new BasicResource
            {
                Name = "alpha",
                Relations = new LinkCollection
                    {
                        {
                            "self", 
                            new Link { Href = "alphahref" }
                        }
                    }
            });

            _list.Items.Add(new BasicResource
            {
                Name = "beta",
                Relations = new LinkCollection
                    {
                        {
                            "self", 
                            new Link { Href = "betahref" }
                        }
                    }
            });

            _list.Items.Add(new BasicResource
            {
                Name = "delta",
                Relations = new LinkCollection
                    {
                        {
                            "self", 
                            new Link { Href = "deltahref" }
                        }
                    }
            });

            _formatter = new CollectionJsonMediaTypeFormatter();
        }



        [TestMethod]
        public void formatter_simpleResourceist_isPopulated()
        {
            // arrange

            // act
            var result = TestHelpers.Format.FormatObject(_list, _formatter);

            // assert
            result.Should().Contain("selfhref");
            result.Should().Contain("alpha");
            result.Should().Contain("beta");
            result.Should().Contain("delta");
        }



        [TestMethod]
        public void formatter_simpleResourceist_dataInValidCj()
        {
            // arrange

            // act
            var result = TestHelpers.Format.FormatObject(_list, _formatter);

            var o = JObject.Parse(result);
            var items = o["collection"]["items"];

            // assert
            items.Should().NotBeNull();
            items.Count().Should().Be(3);
        }
    }
}
