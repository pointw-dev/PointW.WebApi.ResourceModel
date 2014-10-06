using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PointW.WebApi.ResourceModel.TestControllers;

namespace PointW.WebApi.MediaTypeFormatters.Hal.Tests
{
    [TestClass] // ReSharper disable once InconsistentNaming
    public class Tests_HAL03_ContentNegotiationEtc
    {
        private BasicController _controller;
        private HttpServer _server;
        private const string FakeBaseAddress = "http://unit-tester";


        private static HttpResponseMessage GetResponseFromAction(IHttpActionResult action)
        {
            var ct = new CancellationToken();
            return action.ExecuteAsync(ct).Result;
        }



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
            var response = GetResponseFromAction(_controller.GetAllProducts());
            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(200);
        }



        [TestMethod]
        public void SomeController_GetAll_bodyShouldHaveEmbedded()
        {
            // arrange

            // act
            var response = GetResponseFromAction(_controller.GetAllProducts());
            var body = response.Content.ReadAsStringAsync().Result;

            // assert
            body.Should().Contain("_embedded");
        }




        [TestMethod]
        public void SomeController_GetAll_countIs3()
        {
            // arrange

            // act
            var response = GetResponseFromAction(_controller.GetAllProducts());
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
            var response = GetResponseFromAction(_controller.GetProduct(1));
            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(200);
        }



        [TestMethod]
        public void SomeController_GetOneNotExisting_Status404()
        {
            // arrange

            // act
            var response = GetResponseFromAction(_controller.GetProduct(100));
            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(404);
        }



        [TestMethod]
        public void SomeController_GetAcceptBlank_ContentTypeIsHal()
        {
            // arrange

            // act
            var response = GetResponseFromAction(_controller.GetProduct(1));
            var contentTypeHeader = response.Content.Headers.ContentType.ToString();

            // assert
            contentTypeHeader.Should().Contain("application/hal+json");
        }




        [TestMethod]
        public void SomeController_GetAcceptBlank_ContentIsHal()
        {
            // arrange

            // act
            var response = GetResponseFromAction(_controller.GetProduct(1));
            var body = response.Content.ReadAsStringAsync().Result;

            // assert
            body.Should().Contain("_links");
        }




        [TestMethod]
        public void SomeController_GetAcceptHal_ContentTypeIsHal()
        {
            // arrange
            _controller.Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

            // act
            var response = GetResponseFromAction(_controller.GetProduct(1));
            var contentTypeHeader = response.Content.Headers.ContentType.ToString();

            // assert
            contentTypeHeader.Should().Contain("application/hal+json");
        }

        
        
        [TestMethod]
        public void SomeController_GetAcceptJson_ContentTypeIsHal()
        {
            // arrange
            _controller.Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // act
            var response = GetResponseFromAction(_controller.GetProduct(1));
            var contentTypeHeader = response.Content.Headers.ContentType.ToString();

            // assert
            contentTypeHeader.Should().Contain("application/hal+json");
        }



        [TestMethod]
        public void SomeController_GetAcceptXml_ContentTypeIsXml()
        {
            // arrange
            _controller.Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            // act
            var response = GetResponseFromAction(_controller.GetProduct(1));
            var contentTypeHeader = response.Content.Headers.ContentType.ToString();

            // assert
            contentTypeHeader.Should().Contain("application/xml");
        }



        [TestMethod]
        public void SomeController_GetAcceptXml_ContentIsXml()
        {
            // arrange
            _controller.Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            // act
            var response = GetResponseFromAction(_controller.GetProduct(1));
            var body = response.Content.ReadAsStringAsync().Result;

            // assert
            body.Should().Contain("<Name>alpha</Name>");
        }



        [TestMethod]
        public void SomeController_PostStringAsHal_StatusCodeIs201()
        {
            // arrange
            var client = new HttpClient(_server);

            // act
            var response = client.PostAsync(FakeBaseAddress + "/api/basic",
                new StringContent("{make: \"Ford\",model: \"Mustang\"}", Encoding.UTF8, "application/hal+json")).Result;

            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(201);
        }



        [TestMethod]
        public void SomeController_PostWwwFormUrlencoded_StatusCodeIs201()
        {
            // arrange
            var client = new HttpClient(_server);

            // act
            var response = client.PostAsync(FakeBaseAddress + "/api/basic",
                new StringContent("make=Ford&model=Mustang", Encoding.ASCII, "application/x-www-form-urlencoded")).Result;

            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(201);
        }



    }
}
