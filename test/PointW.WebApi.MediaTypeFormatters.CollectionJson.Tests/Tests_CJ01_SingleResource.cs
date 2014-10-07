using System.Linq;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PointW.WebApi.ResourceModel;
using PointW.WebApi.ResourceModel.TestResources;

namespace PointW.WebApi.MediaTypeFormatters.CollectionJson.Tests
{
    [TestClass] // ReSharper disable once InconsistentNaming
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
        public void formatter_withBasic_hasOneItem()
        {
            // arrange
            _basicResource.Relations.Add("self", new Link { Href = "selfhref" });

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            var o = JObject.Parse(result);
            var items = o["collection"]["items"];

            // assert
            items.Should().NotBeNull();
            items.Count().Should().Be(1);
        }



        [TestMethod]
        public void formatter_withBasic_itemsIsArray()
        {
            // arrange

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            var o = JObject.Parse(result);
            var items = o["collection"]["items"];

            // assert
            items.Should().BeOfType<JArray>();
        }



        [TestMethod]
        public void formatter_withBasic_itemHasData()
        {
            // arrange

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            var o = JObject.Parse(result);
            var item = o["collection"]["items"][0];

            // assert
            item.ToString().Should().Contain("data");
        }



        [TestMethod]
        public void formatter_withBasic_dataIsArray()
        {
            // arrange

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            var o = JObject.Parse(result);
            var data = o["collection"]["items"][0]["data"];

            // assert
            data.Should().BeOfType<JArray>();
        }



        [TestMethod]
        public void formatter_withBasic_itemHasNameValuePair()
        {
            // arrange

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            var o = JObject.Parse(result);
            var record = o["collection"]["items"][0]["data"][0];

            // assert
            record["name"].ToString().Should().Be("name");
            record["value"].ToString().Should().Be("Pat Smith");
        }



        [TestMethod]
        public void formatter_withNoLinks_noHrefNoLinks()
        {
            // arrange

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            // assert
            result.Should().NotContain("href");
            result.Should().NotContain("links");
        }



        [TestMethod]
        public void formatter_withNullStringProperty_propertyIsOmitted()
        {
            // arrange
            var resource = new ProductResource
            {
                Make = "Ford"
            };

            // act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);

            // assert
            result.Should().NotContain("model");
            result.Should().NotContain("serialNumber");
        }




        [TestMethod]
        public void formatter_withNeverShowProperty_neverShowPropertyOmitted()
        {
            // arrange
            var resource = new ResourceWithHiddenId
            {
                Id = 1234,
                Name = "Pat Smith"
            };

            // act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);

            // assert
            result.Should().NotContain("internalId");
            result.Should().NotContain("1234");
            result.Should().Contain("Pat Smith");
        }



        [TestMethod]
        public void formatter_withAlwaysShowNulls_showNulls()
        {
            // arrange
            var resource = new ResourceWithAlwaysShowProperty
            {
                Name = "Pat Smith"
            };
            // resource.Name (string) is set and should appear
            // resource.Number (int?) is not set, but should appear because of [AlwaysShow]

            // act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);

            // assert
            result.Should().Contain("Pat Smith");
            result.Should().Contain("number");
        }



        [TestMethod]
        public void formatter_withIntProperty_valueIsInt()
        {
            // arrange
            var resource = new ResourceWithAlwaysShowProperty
            {
                Name = "Pat Smith",
                Number = 42
            };
            // resource.Name (string) is set and should appear
            // resource.Number (int?) is not set, but should appear because of [AlwaysShow]

            // act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);

            // assert
            result.Should().Contain("Pat Smith");
            result.Should().Contain("number");
            result.Should().Contain("42");
            result.Should().NotContain("\"42\"");
            result.Should().NotContain("'42'");
        }



        [TestMethod]
        public void formatter_withCollectionLink_collectionHrefIsCollectionLinkHref()
        {
            // arrange
            _basicResource.Relations.Add("collection", new Link { Href = "collectionhref" });

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            var o = JObject.Parse(result);
            var href = o["collection"]["href"];

            // assert
            href.Should().NotBeNull();
            href.ToString().Should().Be("collectionhref");
        }



        [TestMethod]
        public void formatter_withSelfLink_itemHrefIsSelfLinkHref()
        {
            // arrange
            _basicResource.Relations.Add("self", new Link { Href = "selfhref" });

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            var o = JObject.Parse(result);
            var href = o["collection"]["items"][0]["href"];

            // assert
            href.Should().NotBeNull();
            href.ToString().Should().Be("selfhref");
        }



        [TestMethod]
        public void formatter_withSelfAndCollectionLink_hasBoth()
        {
            // arrange
            _basicResource.Relations.Add("self", new Link { Href = "selfhref" });
            _basicResource.Relations.Add("collection", new Link { Href = "collectionhref" });


            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            var o = JObject.Parse(result);

            var colHref = o["collection"]["href"];
            var selfHref = o["collection"]["items"][0]["href"];

            // assert
            colHref.Should().NotBeNull();
            colHref.ToString().Should().Be("collectionhref");
            selfHref.Should().NotBeNull();
            selfHref.ToString().Should().Be("selfhref");
        }




        [TestMethod]
        public void formatter_withOtherLinks_itemHasLinks()
        {
            // arrange
            _basicResource.Relations.Add("other", new Link { Href = "otherhref" });


            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            var o = JObject.Parse(result);

            var links = o["collection"]["items"][0]["links"];                
            var otherHref = links.First(l => l.Value<string>("rel") == "other")["href"];


            // assert
            otherHref.Should().NotBeNull();
            otherHref.ToString().Should().Be("otherhref");
        }



        [TestMethod]
        public void formatter_withSelfAndOtherLinks_itemHasHrefAndLinks()
        {
            // arrange
            _basicResource.Relations.Add("self", new Link { Href = "selfhref" });
            _basicResource.Relations.Add("other", new Link { Href = "otherhref" });


            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            var o = JObject.Parse(result);

            var links = o["collection"]["items"][0]["links"];
            var otherHref = links.First(l => l.Value<string>("rel") == "other")["href"];
            var selfByRel = links.FirstOrDefault(l => l.Value<string>("rel") == "self");
            var selfHref = o["collection"]["items"][0]["href"].ToString();


            // assert
            selfByRel.Should().BeNull();
            otherHref.Should().NotBeNull();
            otherHref.ToString().Should().Be("otherhref");
            selfHref.Should().Be("selfhref");
        }




        [TestMethod]
        public void formatter_withCollectionAndOtherLinks_itemHasLinksColHasHref()
        {
            // arrange
            _basicResource.Relations.Add("collection", new Link { Href = "collectionhref" });
            _basicResource.Relations.Add("other", new Link { Href = "otherhref" });


            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            var o = JObject.Parse(result);

            var links = o["collection"]["items"][0]["links"];
            var otherHref = links.First(l => l.Value<string>("rel") == "other")["href"];
            var colByRel = links.FirstOrDefault(l => l.Value<string>("rel") == "collection");
            var colHref = o["collection"]["href"].ToString();


            // assert
            colByRel.Should().BeNull();
            otherHref.Should().NotBeNull();
            otherHref.ToString().Should().Be("otherhref");
            colHref.Should().Be("collectionhref");
        }



        [TestMethod]
        public void formatter_withSelfCollectionAndOtherLinks_itemHasSelfAndLinksColHasHref()
        {
            // arrange
            _basicResource.Relations.Add("collection", new Link { Href = "collectionhref" });
            _basicResource.Relations.Add("other", new Link { Href = "otherhref" });
            _basicResource.Relations.Add("self", new Link { Href = "selfhref" });


            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            var o = JObject.Parse(result);

            var links = o["collection"]["items"][0]["links"];
            var otherHref = links.First(l => l.Value<string>("rel") == "other")["href"];
            var colByRel = links.FirstOrDefault(l => l.Value<string>("rel") == "collection");
            var colHref = o["collection"]["href"].ToString();
            var selfByRel = links.FirstOrDefault(l => l.Value<string>("rel") == "self");
            var selfHref = o["collection"]["items"][0]["href"].ToString();



            // assert
            colByRel.Should().BeNull();
            otherHref.Should().NotBeNull();
            otherHref.ToString().Should().Be("otherhref");
            colHref.Should().Be("collectionhref");
            selfByRel.Should().BeNull();
            selfHref.Should().Be("selfhref");
        }



        [TestMethod]
        public void formatter_withMultiPropertyResource_dataHasProperites()
        {
            // arrange
            var product = new ProductResource
            {
                Make = "Ford",
                Model = "Mustang",
                SerialNumber = "VIN123456"
            };

            // act
            var result = TestHelpers.Format.FormatObject(product, _formatter);

            var o = JObject.Parse(result);
            var make = o["collection"]["items"][0]["data"][0]["value"].ToString();
            var model = o["collection"]["items"][0]["data"][1]["value"].ToString();
            var serial = o["collection"]["items"][0]["data"][2]["value"].ToString();

            // assert
            make.Should().Be("Ford");
            model.Should().Be("Mustang");
            serial.Should().Be("VIN123456");
        }


        [TestMethod]
        public void formatter_withResourceFromInterface_sameAsInherited()
        {
            // arrange
            var resource = new ResourceFromInterface
            {
                Name = "Pat Smith",
                Address = "123 Main St."
            };
            resource.Relations.Add("self", new Link { Href = "selfhref" });

            // act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);

            var o = JObject.Parse(result);
            var item = o["collection"]["items"][0];
            var data = item["data"];

            var name = data.First(d => d.Value<string>("name") == "name")["value"].ToString();
            var address = data.First(d => d.Value<string>("name") == "address")["value"].ToString();
            var phone = data.FirstOrDefault(d => d.Value<string>("name") == "phone");
            var selfHref = item["href"].ToString();

            // assert
            name.Should().Be("Pat Smith");
            address.Should().Be("123 Main St.");
            phone.Should().BeNull();
            selfHref.Should().Be("selfhref");
        }

    }
}
