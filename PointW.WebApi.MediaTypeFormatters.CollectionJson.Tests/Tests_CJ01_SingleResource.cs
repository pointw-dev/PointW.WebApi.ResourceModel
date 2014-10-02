using System;
using System.IO;
using System.Net.Http.Formatting;
using System.Text;
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
            var result = FormatObject(_basicResource, _formatter);

            // assert
            result.Should().NotBeEmpty();
        }



        private static string FormatObject(object toFormat, BaseJsonMediaTypeFormatter formatter)
        {
            string result;
            using (var stream = new MemoryStream())
            {
                formatter.WriteToStream(toFormat.GetType(), toFormat, stream, new UTF8Encoding());
                stream.Seek(0, SeekOrigin.Begin);
                result = new StreamReader(stream).ReadToEnd();
            }
            return result;
        }

    }
}
