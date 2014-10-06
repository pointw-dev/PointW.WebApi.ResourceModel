using System.Collections.Generic;
using System.Linq;
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
        // private SimpleResourceList<BasicResource> _list;
        private SimpleResourceList2 _list;



        [TestInitialize]
        public void Setup()
        {
            _list = new SimpleResourceList2
            {
                Relations = new LinkCollection { { "self", new Link { Href = "selfhref" } } }, 
                Items = new List<Resource>()
            };

            // _list = new SimpleResourceList<BasicResource>
            // {
            //     Relations = new LinkCollection { { "self", new Link { Href = "selfhref" } } },
            //     Items = new List<BasicResource>()
            // };

            _list.Items.Add(new BasicResource
            {
                Name = "alpha",
                Relations = new LinkCollection { { "self", new Link { Href = "alphahref" } } }
            });

            _list.Items.Add(new BasicResource
            {
                Name = "beta",
                Relations = new LinkCollection { { "self", new Link { Href = "betahref" } } }
            });

            _list.Items.Add(new BasicResource
            {
                Name = "gamma",
                Relations = new LinkCollection { { "self", new Link { Href = "gammahref" } } }
            });

            _formatter = new CollectionJsonMediaTypeFormatter();
        }



        [TestMethod]
        public void formatter_simpleResourceList_hasSelfLink()
        {
            // arrange

            // act
            var result = TestHelpers.Format.FormatObject(_list, _formatter);

            // assert
            result.Should().Contain("selfhref");
        }



        [TestMethod]
        public void formatter_simpleResourceList_itemsCountIs3()
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


        
        [TestMethod]
        public void formatter_simpleResourceList_eachHasItsName()
        {
            // arrange
        
            // act
            var result = TestHelpers.Format.FormatObject(_list, _formatter);

            var o = JObject.Parse(result);
            var items = o["collection"]["items"];
            var alpha = o["collection"]["items"][0]["data"][0];
            var beta = o["collection"]["items"][1]["data"][0];
            var gamma = o["collection"]["items"][2]["data"][0];

            // assert
            alpha["name"].ToString().Should().Be("name");
            beta["name"].ToString().Should().Be("name");
            gamma["name"].ToString().Should().Be("name");
            alpha["value"].ToString().Should().Be("alpha");
            beta["value"].ToString().Should().Be("beta");
            gamma["value"].ToString().Should().Be("gamma");
        }

        
        
        [TestMethod]
        public void formatter_simpleResourceList_eachItemHasHref()
        {
            // arrange

            // act
            var result = TestHelpers.Format.FormatObject(_list, _formatter);

            var o = JObject.Parse(result);
            var items = o["collection"]["items"];
            var alphahref = items[0]["href"].ToString(); // TODO: get by name rather than by position
            var betahref = items[1]["href"].ToString();
            var gammahref = items[2]["href"].ToString();

            // assert
            alphahref.Should().Be("alphahref");
            betahref.Should().Be("betahref");
            gammahref.Should().Be("gammahref");
        }



        [TestMethod]
        public void formatter_withOtherLinkOnAlpha_alphaHasLinks()
        {
            // arrange
            var alpha = _list.Items.FirstOrDefault(i => ((BasicResource) i).Name == "alpha");
            alpha.Relations.Add("other", new Link {Href = "otherhref"});

            // act
            var result = TestHelpers.Format.FormatObject(_list, _formatter);

            var o = JObject.Parse(result);
            var items = o["collection"]["items"];
            var alphaLinks = items[0]["links"]; // TODO: get by name rather than by position
            var otherLink = alphaLinks.FirstOrDefault(l => l.Value<string>("rel") == "other");

            // assert
            alphaLinks.Should().NotBeNull();
            otherLink["href"].ToString().Should().Be("otherhref");
        }



        [TestMethod]
        public void formatter_withOtherLinkOnAlpha_linksOmitsSelf()
        {
            // arrange
            var alpha = _list.Items.FirstOrDefault(i => ((BasicResource)i).Name == "alpha");
            alpha.Relations.Add("other", new Link { Href = "otherhref" });

            // act
            var result = TestHelpers.Format.FormatObject(_list, _formatter);

            var o = JObject.Parse(result);
            var items = o["collection"]["items"];
            var alphaLinks = items[0]["links"]; // TODO: get by name rather than by position
            var alphaSelf = alphaLinks.FirstOrDefault(l => l.Value<string>("rel") == "self");
            // assert
            alphaLinks.Should().NotBeNull();
            alphaSelf.Should().BeNull();
        }
    
        // TODO: what about collection link in data? s/b omitted too?  s/b first checked if == collection.href??
    }
}
