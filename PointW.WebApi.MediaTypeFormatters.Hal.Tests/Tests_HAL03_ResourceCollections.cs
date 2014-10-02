using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using PointW.WebApi.ResourceModel.TestResources;
using PointW.WebApi.ResourceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;

namespace PointW.WebApi.MediaTypeFormatters.Hal.Tests
{
    [TestClass]
    public class Tests_HAL03_ResourceCollections
    {
        private HalJsonMediaTypeFormatter _formatter;
        private SimpleResourceList<BasicResource> _list;



        [TestInitialize]
        public void Setup()
        {
            _list = new SimpleResourceList<BasicResource>();
            _list.Relations.Add("self", new Link{ Href = "selfhref"});

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

            _formatter = new HalJsonMediaTypeFormatter();
        }



        [TestMethod]
        public void formatter_simpleResourceist_isPopulated()
        {
            // arrange

            // act
            var result = TestHelpers.Format.FormatObject(_list, _formatter);

            // assert
            result.Should().Contain("selfhref");
            result.Should().Contain("_embedded");
            result.Should().Contain("alpha");
            result.Should().Contain("beta");
            result.Should().Contain("delta");
        }



        [TestMethod]
        public void formatter_simpleResourceist_dataInValidJson()
        {
            // arrange

            // act
            var result = TestHelpers.Format.FormatObject(_list, _formatter);

            var o = JObject.Parse(result);
            var items = o["_embedded"]["items"];
            var alpha = items.First(i => i["name"].ToString() == "alpha");
            var alphaLink = alpha["_links"]["self"]["href"];

            // assert
            alpha["name"].ToString().Should().Be("alpha");
            alphaLink.ToString().Should().Be("alphahref");
        }



        [TestMethod]
        public void formatter_withEmbeddedLists_singularNamedEmbeddedLists()
        {
            // arrange
            var resource = new ResourceWithEmbeddedLists
            {
                Name = "Office Inventory",
                Cars = new List<ProductResource>
                {
                    new ProductResource
                    {
                        Make = "Ford",
                        Model = "Mustang"
                    },
                    new ProductResource
                    {
                        Make = "Dodge",
                        Model = "Magnum"
                    }
                },
                Computers = new List<ProductResource>
                {
                    new ProductResource
                    {
                        Make = "Dell"
                    },
                    new ProductResource
                    {
                        Make = "IBM"
                    },
                    new ProductResource
                    {
                        Make = "Acer"
                    }
                }
            };

            // act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);

            var o = JObject.Parse(result);
            var cars = o["_embedded"]["cars"];
            var computers = o["_embedded"]["computers"];

            // assert
            cars.Count().Should().Be(2);
            computers.Count().Should().Be(3);
        }
    }
}
