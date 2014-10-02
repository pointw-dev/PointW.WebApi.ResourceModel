using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}
