using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PointW.WebApi.ResourceModel;
using PointW.WebApi.ResourceModel.TestResources;

namespace PointW.WebApi.MediaTypeFormatters.CollectionJson.Tests
{
    [TestClass]
    public class Tests_CJ01_SingleResource
    {
        private BasicResource _basicResource;
        private CollectionJsonMediaTypeFormatter _formatter;



        [TestInitialize]
        public void Setup()
        {
            _basicResource = new BasicResource
            {
                Name = "Pat Smith"
            };

            _formatter = new CollectionJsonMediaTypeFormatter();
        }



        [TestMethod]
        public void formatter_withBasic_isPopulated()
        {
            // arrange
            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            // assert
            result.Should().NotBeEmpty();
        }

        
        
        [TestMethod]
        public void formatter_withBasic_isCollection()
        {
            // arrange
            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            var o = JObject.Parse(result);
            var collection = o["collection"];

            // assert
            collection.Should().NotBeNull();
            o.Count.Should().Be(1);
        }



        [TestMethod]
        public void formatter_withSelfLink_containsLinks()
        {
            // arrange
            _basicResource.Relations.Add("self", new Link { Href = "selfhref"});

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            var o = JObject.Parse(result);
            var links = o["collection"]["links"];

            // assert
            links.Should().NotBeNull();
        }



        [TestMethod]
        public void formatter_withSelfLink_containsValidCjSelfLink()
        {
            // arrange
            _basicResource.Relations.Add("self", new Link { Href = "selfhref" });
        
            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);
        
            var o = JObject.Parse(result);
            var links = o["collection"]["links"];
            var selfLink = links.FirstOrDefault(l => l.Value<string>("rel") == "self");
            var href = selfLink["href"].ToString();
        
            // assert
            selfLink.Should().NotBeNull();
            href.Should().Be("selfhref");
        }



        [TestMethod]
        public void formatter_withBasic_hasOneItem()
        {
            // arrange
            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);
        
            var o = JObject.Parse(result);
            var items = o["collection"]["items"];
        
            // assert
            items.Should().NotBeNull();
            items.Count().Should().Be(1);
        }
    
    }
}
