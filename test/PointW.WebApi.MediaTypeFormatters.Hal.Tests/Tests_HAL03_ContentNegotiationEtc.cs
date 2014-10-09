using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PointW.WebApi.ResourceModel.TestControllers;

namespace PointW.WebApi.MediaTypeFormatters.Hal.Tests
{
    [TestClass] // ReSharper disable once InconsistentNaming
    public class Tests_HAL03_Controller
    {
        private BasicController _controller;
        private HttpServer _server;
        private const string FakeBaseAddress = "http://unit-tester";


        [TestInitialize]
        public void Setup()
        {
            _controller = new BasicController
            {
                Request = new HttpRequestMessage()
            };

            var config = new HttpConfiguration();

            // configuration to allow for route/link-gen testing
            config.MapHttpAttributeRoutes();
            config.EnsureInitialized();

            config.Formatters.Clear();
            config.Formatters.Add(new HalJsonMediaTypeFormatter());
            config.Formatters.Add(new XmlMediaTypeFormatter());
            config.Formatters.Add(new JQueryMvcFormUrlEncodedFormatter(config));

            _server = new HttpServer(config);

            _controller.Configuration = config;

            _controller.Request.RequestUri = new Uri(FakeBaseAddress); // effectively set the Base URL so Url.Link() can produce fq hrefs
        }



        [TestMethod]
        public void SomeController_GetAll_Status200()
        {
            // arrange

            // act
            var response = TestHelpers.Format.GetResponseFromAction(_controller.GetAllProducts());
            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(200);
        }



        [TestMethod]
        public void SomeController_GetAll_bodyShouldHaveEmbedded()
        {
            // arrange

            // act
            var response = TestHelpers.Format.GetResponseFromAction(_controller.GetAllProducts());
            var body = response.Content.ReadAsStringAsync().Result;

            // assert
            body.Should().Contain("_embedded");
        }




        [TestMethod]
        public void SomeController_GetAll_countIs3()
        {
            // arrange

            // act
            var response = TestHelpers.Format.GetResponseFromAction(_controller.GetAllProducts());
            var body = response.Content.ReadAsStringAsync().Result;

            var o = JObject.Parse(body);
            var count = (int)o["count"];
            
            // assert
            count.Should().Be(3);
        }



        [TestMethod]
        public void SomeController_GetOneExisting_Status200()
        {
            // arrange

            // act
            var response = TestHelpers.Format.GetResponseFromAction(_controller.GetProduct(1));
            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(200);
        }



        [TestMethod]
        public void SomeController_GetOneNotExisting_Status404()
        {
            // arrange

            // act
            var response = TestHelpers.Format.GetResponseFromAction(_controller.GetProduct(100));
            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(404);
        }
    }
}
