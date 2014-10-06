using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using PointW.WebApi.ResourceModel.TestResources;
using PointW.WebApi.ResourceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PointW.WebApi.MediaTypeFormatters.Hal.Tests
{
    [TestClass]
    public class Tests_HAL04_RoundTripFormatting
    {
        private HalJsonMediaTypeFormatter _formatter;

        [TestInitialize]
        public void Setup()
        {
            _formatter = new HalJsonMediaTypeFormatter();
        }

        [TestMethod]
        public void roundTrip_basic_typeAndDataCorrect()
        {
            // arrange
            var test = new BasicResource
            {
                Name = "Pat Smith"
            };

            // act
            var result = TestHelpers.Format.PerformRoundTrip<BasicResource>(test, _formatter);

            // assert
            result.Name.Should().Be("Pat Smith");
            result.Should().BeOfType(typeof (BasicResource));
        }



        [TestMethod]
        public void roundTrip_embedded_subResourcesCorrect()
        {
            // arrange
            var test = new ResourceWithEmbeddedProducts()
            {
                Name = "foobar",
                Car = new ProductResource
                {
                    Make = "Ford",
                    Model = "Mustang"
                }
            };

            // act
            var result = TestHelpers.Format.PerformRoundTrip<ResourceWithEmbeddedProducts>(test, _formatter);

            // assert
            result.Should().BeOfType<ResourceWithEmbeddedProducts>();
            result.Name.Should().Be("foobar");
            result.Car.Make.Should().Be("Ford");
            result.Car.Should().BeOfType<ProductResource>();
        }



        [TestMethod]
        public void roundTrip_nestedEmbeds_structureRecursed()
        {
            // arrange
            var test = new RecursiveResource
            {
                Name = "Outermost",
                NestedResource = new RecursiveResource
                {
                    Name = "Mid Level",
                    NestedResource = new RecursiveResource
                    {
                        Name = "Innermost"
                    }
                }
            };

            // act
            var result = TestHelpers.Format.PerformRoundTrip<RecursiveResource>(test, _formatter);
            var mid = result.NestedResource;
            var inner = mid.NestedResource;

            // assert
            result.Should().BeOfType<RecursiveResource>();
            mid.Should().BeOfType<RecursiveResource>();
            inner.Should().BeOfType<RecursiveResource>();
            result.Name.Should().Be("Outermost");
            mid.Name.Should().Be("Mid Level");
            inner.Name.Should().Be("Innermost");
        }



        [TestMethod]
        public void roundTrip_simpleList_listReconstituted()
        {
            // arrange
            var testList = new SimpleResourceList<BasicResource>();

            testList.Items.Add(new BasicResource
            {
                Name = "alpha",
            });

            testList.Items.Add(new BasicResource
            {
                Name = "beta",
            });

            testList.Items.Add(new BasicResource
            {
                Name = "delta",
            });

            // act
            var obj = TestHelpers.Format.PerformRoundTrip<SimpleResourceList<BasicResource>>(testList, _formatter);

            // assert
            obj.Should().BeOfType<SimpleResourceList<BasicResource>>();
            obj.Count.Should().Be(3);
            obj.Items.Count.Should().Be(3);
        }



        [TestMethod]
        public void roundTrip_typedList_listReconstituted()
        {
            // arrange
            var testList = new TypedResourceList<BasicResource>(new List<BasicResource>());

            testList.Items.Add(new BasicResource
            {
                Name = "alpha",
            });

            testList.Items.Add(new BasicResource
            {
                Name = "beta",
            });

            testList.Items.Add(new BasicResource
            {
                Name = "delta",
            });

            // act
            var obj = TestHelpers.Format.PerformRoundTrip<TypedResourceList<BasicResource>>(testList, _formatter);

            // assert
            obj.Should().BeOfType<TypedResourceList<BasicResource>>();
            obj.Items.Count.Should().Be(3);
        }



        /// <summary>
        /// If a derived object is added to a generic superclass resource list, it will
        /// serialize as the derived object, but will deserialize as the superclass, losing
        /// any subclassed data.
        /// </summary>
        [TestMethod]
        public void roundTrip_heteroListAsUniform_listReconstituted()
        {
            // arrange
            var testList = new SimpleResourceList<BasicResource>();
            
            testList.Items.Add(new BasicResource
            {
                Name = "alpha",
            });
            
            testList.Items.Add(new DerivedResource
            {
                Name = "beta",
                Extra = "omega"
            });
            
            testList.Items.Add(new BasicResource
            {
                Name = "delta",
            });
            
            // act
            var obj = TestHelpers.Format.PerformRoundTrip<SimpleResourceList<BasicResource>>(testList, _formatter);
            var alpha = obj.Items.First(i => i.Name == "alpha");
            var beta = obj.Items.First(i => i.Name == "beta");
            
            // assert
            obj.Should().BeOfType<SimpleResourceList<BasicResource>>();
            obj.Count.Should().Be(3);
            obj.Items.Count.Should().Be(3);
            alpha.Name.Should().Be("alpha");
            alpha.Should().BeOfType<BasicResource>();
            beta.Name.Should().Be("beta");
            beta.Should().BeOfType<BasicResource>(); // which means Extra is gone
        }

        
        
        [TestMethod]
        public void roundTrip_heteroListAsHetero_listReconstituted()
        {
            // arrange
            var testList = new SimpleResourceList<BasicResource>();

            testList.Items.Add(new BasicResource
            {
                Name = "alpha",
            });

            testList.Items.Add(new DerivedResource
            {
                Name = "beta",
                Extra = "omega"
            });

            testList.Items.Add(new BasicResource
            {
                Name = "delta",
            });

            // act
            var obj = TestHelpers.Format.PerformRoundTrip<SimpleResourceList<BasicResource>>(testList, _formatter);
            var beta = obj.Items.First(i => i.Name == "beta") as DerivedResource;

            // assert
            Assert.Inconclusive("Is this required functionality?  If not, can ILists enforce homogeneity?");
            obj.Should().BeOfType<SimpleResourceList<BasicResource>>();
            obj.Count.Should().Be(3);
            obj.Items.Count.Should().Be(3);
            beta.Name.Should().Be("beta");
            beta.Extra.Should().Be("omega");
            beta.Should().BeOfType<DerivedResource>();
        }
    }
}
