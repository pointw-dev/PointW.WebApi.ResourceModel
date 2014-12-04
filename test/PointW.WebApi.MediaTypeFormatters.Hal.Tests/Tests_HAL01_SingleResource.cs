using System;
using System.Linq;
using FluentAssertions;
using PointW.WebApi.ResourceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PointW.WebApi.ResourceModel.TestResources;

namespace PointW.WebApi.MediaTypeFormatters.Hal.Tests
{
    /// <summary>
    /// By Single resource I mean "not in a list".  Some resources
    /// tested here have embedded resources.
    /// NOTE: when the "action" part of the test name begins with "with...", read it as if
    /// the name was "format a resource with..."
    /// </summary>
    [TestClass] // ReSharper disable once InconsistentNaming
    public class Tests_HAL01_SingleResource
    {
        private BasicResource _basicResource;
        private HalJsonMediaTypeFormatter _formatter;



        [TestInitialize]
        public void Setup()
        {
            _basicResource = new BasicResource
            {
                Name = "Pat Smith"
            };

            _formatter = new HalJsonMediaTypeFormatter();
        }



        [TestMethod]
        public void formatter_withBasic_isPopulated()
        {
            // arrange

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            // assert
            result.Should().NotBeEmpty();
            result.Should().NotContain("_embedded");
            result.Should().NotContain("curies");
        }


        /// <summary>
        /// This test and the next results from a design principle that all Resources
        /// should contain links.  The spec (draft-kelly-json-hal-06) says links are 
        /// optional.  To turn links off with this implementation requires an explicit
        /// intent, i.e. set Relations = null
        /// </summary>
        [TestMethod]
        public void formatter_withBasic_containsLinks()
        {
            // arrange

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            // assert
            result.Should().Contain("_links");
        }



        [TestMethod]
        public void formatter_withLinksExplicitlyNulled_containsNoLinks()
        {
            // arrange
            _basicResource.Relations = null;

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            // assert
            result.Should().NotContain("_links");
        }



        [TestMethod]
        public void formatter_withBasic_dataIsJsonParsable()
        {
            // arrange
            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            var o = JObject.Parse(result);
            var name = (string)o["name"];

            // assert
            name.Should().Be("Pat Smith");
        }



        [TestMethod]
        public void formatter_withSelfLink_containsHalValidSelfLink()
        {
            // arrange
            _basicResource.Relations.Add("self", new Link { Href = "selfhref" });

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);
            var o = JObject.Parse(result);
            var self = o["_links"]["self"];
            var href = (string)self["href"];

            // assert
            href.Should().Be("selfhref");
        }



        [TestMethod]
        public void formatter_withMultipleLinks_format_allLinksHalValid()
        {
            // arrange
            _basicResource.Relations.Add("self", new Link { Href = "selfhref" });
            _basicResource.Relations.Add("boss", new Link { Href = "bosshref" });

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            var o = JObject.Parse(result);
            var self = o["_links"]["self"];
            var boss = o["_links"]["boss"];
            
            var selfHref = (string)self["href"];
            var bossHref = (string)boss["href"];

            // assert
            o["_links"].Count().Should().Be(2);
            selfHref.Should().Be("selfhref");
            bossHref.Should().Be("bosshref");
        }



        [TestMethod]
        public void formatter_withNotTemplatedLink_linkHasNoTemplatedField()
        {
            // arrange
            _basicResource.Relations.Add("self", new Link { Href = "selfhref" });

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            // assert
            result.Should().NotContain("templated");
        }



        /// <summary>
        /// The link's templated property is described in sec 5.2 of
        /// draft-kelly-json-hal-06
        /// </summary>
        [TestMethod]
        public void formatter_withTemplatedHref_linkTemplatedIsTrue()
        {
            // arrange
            _basicResource.Relations.Add("boss", new Link { Href = "http://somedomain/api/boss/{bossId}" });

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);
            
            var o = JObject.Parse(result);
            var boss = o["_links"]["boss"];
            var isTemplated = (bool)boss["templated"];

            // assert
            isTemplated.Should().BeTrue();
        }



        [TestMethod]
        public void formatter_withNotDeprecatedLink_linkHasNoDeprecationField()
        {
            // arrange
            _basicResource.Relations.Add("self", new Link { Href = "selfhref" });

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            // assert
            result.Should().NotContain("deprecation");
        }


        /// <summary>
        /// The link's deprecation property is described in sec 5.4 of
        /// draft-kelly-json-hal-06
        /// </summary>
        [TestMethod]
        public void formatter_withDeprecatedLink_linkDeprecationIsTrue()
        {
            // arrange
            _basicResource.Relations.Add("boss", new Link
            {
                Href = "http://somedomain/api/boss/{bossId}",
                IsDeprecated = true
            });

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            var o = JObject.Parse(result);
            var boss = o["_links"]["boss"];
            var isDeprecated = (bool)boss["deprecation"];

            // assert
            isDeprecated.Should().BeTrue();
        }



        [TestMethod]
        public void formatter_formatNonResource_hasNoHalFeatures()
        {
            // arrange
            var resource = new {Name = "Pat"};

            //act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);            

            // assert
            result.Should().NotContain("_links");
            result.Should().NotContain("_embedded");
            result.Should().NotContain("curies");
        }



        [TestMethod]
        public void formatter_formatNonResourceWithNestedResource_resourceHasHalFeatures()
        {
            // arrange
            var resource = new { Name = "Pat", Car = new BasicResource { Name = "Pat Smith" } };

            //act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);

            // assert
            result.Should().Contain("_links");
            result.Should().Contain("Pat Smith");
        }



        [TestMethod]
        public void formatter_withNullStringProperty_propertyIsOmitted()
        {
            // arrange
            _basicResource.Name = null;

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            // assert
            result.Should().NotContain("name");
        }



        /// <summary>
        /// The rationale for this behaviour is that uninitialized string properties are null and should
        /// not be emitted.  However, if the program sets a string to "", that action signals an intent
        /// that should be emitted.  To signal the intent to erase a string, set it to null.
        /// </summary>
        [TestMethod]
        public void formatter_withEmptyString_showEmptyString()
        {
            // arrange
            _basicResource.Name = "";

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            // assert
            result.Should().Contain("name");
        }



        [TestMethod]
        public void formatter_withNullableProperties_omitNullValues()
        {
            // arrange
            var resource = new ResourceWithNullables();
            // resource.Name (string) and resource.Number (int?) are not set and should not appear

            // act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);

            // assert
            result.Should().NotContain("name");
            result.Should().NotContain("number");
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
            result.Should().Contain("_links");
        }



        [TestMethod]
        public void formatter_withPopulatedNullableProperties_showsPopulatedNullables()
        {
            // arrange
            var resource = new ResourceWithNullables
            {
                Name = "Pat Smith",
                Number = 42
            };

            // act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);

            // assert
            result.Should().Contain("name");
            result.Should().Contain("number");
        }


        
        [TestMethod]
        public void formatter_withAlwaysShowNulls_showNulls()
        {
            // arrange
            var resource = new ResourceWithAlwaysShowProperty();
            // resource.Name (string) is not set and should not appear
            // resource.Number (int?) is not set, but should appear because of [AlwaysShow]

            // act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);

            // assert
            result.Should().NotContain("name");
            result.Should().Contain("number");
        }



        [TestMethod]
        public void formatter_withUnassignedEmbeddedResource_omitEmbedded()
        {
            // arrange
            var resource = new ResourceWithEmbeddedProducts
            {
                Name = "Pat Smith"
            };

            // act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);

            // assert
            result.Should().NotContain("_embedded");
            result.Should().Contain("Pat Smith");
        }




        [TestMethod]
        public void formatter_withEmbeddedResource_emitsEmbeddedAndData()
        {
            // arrange
            var resource = new ResourceWithEmbeddedProducts
            {
                Car = new ProductResource
                {
                    Make = "Ford"
                }
            };
        
            // act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);
            
            // assert
            result.Should().Contain("_embedded");
            result.Should().Contain("Ford");
        }



        [TestMethod]
        public void formatter_withEmbeddedResource_emitsValidHalEmbedded()
        {
            // arrange
            var resource = new ResourceWithEmbeddedProducts
            {
                Car = new ProductResource
                {
                    Make = "Ford"
                }
            };

            // act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);

            var o = JObject.Parse(result);
            var embedded = o["_embedded"];
            var make = embedded["car"]["make"];


            // assert
            embedded.Count().Should().Be(1);
            make.ToString().Should().Be("Ford");
        }



        [TestMethod]
        public void formatter_withMultipleEmbeddedResources_emitsEmbeddedAndData()
        {
            // arrange
            var resource = new ResourceWithEmbeddedProducts
            {
                Car = new ProductResource
                {
                    Make = "Ford"
                },
                Computer = new ProductResource
                {
                    Make = "Acer"
                }
            };

            // act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);

            // assert
            result.Should().Contain("_embedded");
            result.Should().Contain("Ford");
            result.Should().Contain("Acer");
        }



        [TestMethod]
        public void formatter_withMultipleEmbeddedResources_emitsValidHalEmbedded()
        {
            // arrange
            var resource = new ResourceWithEmbeddedProducts
            {
                Car = new ProductResource
                {
                    Make = "Ford"
                },
                Computer = new ProductResource
                {
                    Make = "Acer"
                }
            };

            // act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);

            var o = JObject.Parse(result);
            var embedded = o["_embedded"];
            var carMake = embedded["car"]["make"];
            var computerMake = embedded["computer"]["make"];


            // assert
            embedded.Count().Should().Be(2);
            carMake.ToString().Should().Be("Ford");
            computerMake.ToString().Should().Be("Acer");
        }



        [TestMethod]
        public void formatter_withMultiTypesEmbedded_emitsValidHal()
        {
            // arrange
            var resource = new ResourceWithMultitypeEmbeddeds
            {
                Name = "Car Assignment",
                Car = new ProductResource
                {
                    Make = "Ford"
                },
                Employee = new BasicResource
                {
                    Name = "Pat Smith"
                }
            };

            // act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);

            var o = JObject.Parse(result);
            var embedded = o["_embedded"];
            var car = embedded["car"];
            var employee = embedded["employee"];

            // assert
            embedded.Count().Should().Be(2);
            car["make"].ToString().Should().Be("Ford");
            employee["name"].ToString().Should().Be("Pat Smith");
            o["name"].ToString().Should().Be("Car Assignment");
        }



        [TestMethod]
        public void formatter_withEmbeddedsHavingLinks_emitsValidHal()
        {
            // arrange
            var resource = new ResourceWithEmbeddedProducts
            {
                Car = new ProductResource
                {
                    Make = "Ford"
                }
            };

            resource.Car.Relations.Add("warranty", new Link { Href = "warrantyLink" });

            // act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);

            var o = JObject.Parse(result);
            var embedded = o["_embedded"];
            var carLinks = embedded["car"]["_links"];


            // assert
            embedded.Count().Should().Be(1);
            carLinks["warranty"]["href"].ToString().Should().Be("warrantyLink");
        }





        [TestMethod]
        public void formatter_withNestedEmbeddeds_embeddedsProperlyNested()
        {
            // arrange
            var outerResource = new RecursiveResource
            {
                Name = "Base"
            };

            var mid = new RecursiveResource
            {
                Name = "Middle"
            };

            var inner = new RecursiveResource
            {
                Name = "Inner"
            };

            mid.NestedResource = inner;
            outerResource.NestedResource = mid;


            // act
            var result = TestHelpers.Format.FormatObject(outerResource, _formatter);

            var o = JObject.Parse(result);
            var innerEmbedded = o["_embedded"]["nestedResource"]["_embedded"]["nestedResource"];

            // assert
            result.Should().Contain("_embedded");
            result.Should().Contain("Base");
            result.Should().Contain("Middle");
            result.Should().Contain("Inner");
            innerEmbedded["name"].ToString().Should().Be("Inner");
        }



        [TestMethod]
        public void formatter_withRelationQualifier_emitsCuries()
        {
            // arrange
            _basicResource.Relations.AddQualifier("eg", "http://example.org/relations/{rel}");
            _basicResource.Relations.Add("eg:somerel", new Link { Href = "somelinkref" });

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            // assert
            result.Should().Contain("curies");
        }



        [TestMethod]
        public void formatter_withRelationQualifier_curieIsArray()
        {
            // arrange
            _basicResource.Relations.AddQualifier("eg", "http://example.org/relations/{rel}");
            _basicResource.Relations.Add("eg:somerel", new Link { Href = "somelinkref" });

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);
            
            var o = JObject.Parse(result);
            var curies = o["_links"]["curies"];

            // assert
            curies.Should().NotBeNull();
            curies.GetType().Should().Be(typeof (JArray));
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
            resource.Relations.Add("self", new Link{Href = "selfhref"});

            // act
            var result = TestHelpers.Format.FormatObject(resource, _formatter);

            var o = JObject.Parse(result);
            var name = o["name"].ToString();
            var address = o["address"].ToString();
            var phone = o["phone"];
            var selfLink = o["_links"]["self"];

            // assert
            name.Should().Be("Pat Smith");
            address.Should().Be("123 Main St.");
            phone.Should().BeNull();
            selfLink["href"].ToString().Should().Be("selfhref");
        }




        [TestMethod]
        public void formatter_withRelationQualifer_curieIsHalValid()
        {
            // arrange
            _basicResource.Relations.AddQualifier("eg", "http://example.org/relations/{rel}");
            _basicResource.Relations.Add("eg:somerel", new Link { Href = "somelinkref" });

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, _formatter);

            var o = JObject.Parse(result);
            var curies = o["_links"]["curies"];

            // assert
            curies.Count().Should().Be(1);
            curies[0]["name"].ToString().Should().Be("eg");
        }



        [TestMethod]
        public void formatter_withIndentOverrideToFalse_jsonIsOneLine()
        {
            // arrange
            var noIndentFormatter = new HalJsonMediaTypeFormatter {Indent = false};

            // act
            var result = TestHelpers.Format.FormatObject(_basicResource, noIndentFormatter);
            var lines = result.Count(c => c == '\n');
            
            // assert
            lines.Should().BeLessOrEqualTo(1);
        }



        [TestMethod, Ignore]
        public void WhatAboutMultipleLinkRels()
        {
            // TODO: we will never need this, but the spec provides for it.  Should we implement it?

            // "admin": [{
            //     "href": "/admins/2",
            //     "title": "Fred"
            // }, {
            //     "href": "/admins/5",
            //     "title": "Kate"
            // }]


            // {
            //     "_links": {
            //       "item": [{
            //           "href": "/first_item"
            //       },{
            //           "href": "/second_item"
            //       }]
            //     }
            // }

        }
    }
}
