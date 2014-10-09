using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PointW.WebApi.MediaTypeFormatters.CollectionJson;
using PointW.WebApi.MediaTypeFormatters.Hal;
using PointW.WebApi.ResourceModel.TestControllers;

namespace PointW.WebApi.MediaTypeFormatters.Config.Tests
{
    [TestClass] // ReSharper disable once InconsistentNaming
    public class Tests_Config01_ContentNegotiation
    {
        private HttpConfiguration _config;
        private BasicController _controller;
        private HttpServer _server;
        private HttpClient _client;

        private const string FakeBaseAddress = "http://unit-tester";



        private void ConfigureHalExclusively(bool asJson = false)
        {
            _config.Formatters.Clear();
            _config.Formatters.Add(new HalJsonMediaTypeFormatter(asJson: asJson));
            _config.Services.Replace(typeof(IContentNegotiator), new DefaultContentNegotiator(excludeMatchOnTypeOnly: true));
        }



        private void ConfigureCjExclusively(bool asJson = false)
        {
            _config.Formatters.Clear();
            _config.Formatters.Add(new CollectionJsonMediaTypeFormatter(asJson: asJson));
            _config.Services.Replace(typeof(IContentNegotiator), new DefaultContentNegotiator(excludeMatchOnTypeOnly: true));
        }



        private void ConfigureHalThenCj()
        {
            _config.Formatters.Clear();
            _config.Formatters.Add(new HalJsonMediaTypeFormatter());
            _config.Formatters.Add(new CollectionJsonMediaTypeFormatter());
        }




        private void ConfigureCjThenHal()
        {
            _config.Formatters.Clear();
            _config.Formatters.Add(new HalJsonMediaTypeFormatter());
            _config.Formatters.Add(new CollectionJsonMediaTypeFormatter());
        }




        [TestInitialize]
        public void Setup()
        {
            _config = new HttpConfiguration();

            // configuration to allow for route/link-gen testing
            _config.MapHttpAttributeRoutes();
            _config.EnsureInitialized();

            _controller = new BasicController
            {
                Request = new HttpRequestMessage(),
                Configuration = _config
            };

            _controller.Request.RequestUri = new Uri(FakeBaseAddress); // effectively set the Base URL so Url.Link() can produce fq hrefs

            _server = new HttpServer(_config);
            _client = new HttpClient(_server);
        }



        [TestMethod]
        public void configDefault_GetNoAccept_statusCodeIs200()
        {
            // arrange

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(200);
        }



        [TestMethod]
        public void configDefault_GetNoAccept_bodyIsNotHal()
        {
            // arrange

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var body = response.Content.ReadAsStringAsync().Result;

            // assert
            body.Should().NotContain("_links");
        }



        [TestMethod]
        public void configNoFormatters_GetAcceptJson_statusCodeIs406()
        {
            // arrange
            _config.Formatters.Clear();

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(406);
        }




        [TestMethod]
        public void configOnlyCj_GetNoAccept_statusCodeIs200()
        {
            // arrange
            _config.Formatters.Clear();
            _config.Formatters.Add(new CollectionJsonMediaTypeFormatter());

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(200);
        }



        [TestMethod]
        public void configOnlyCj_GetNoAccept_bodyIsCj()
        {
            // arrange
            _config.Formatters.Clear();
            _config.Formatters.Add(new CollectionJsonMediaTypeFormatter());

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var body = response.Content.ReadAsStringAsync().Result;

            // assert
            body.Should().Contain("links");
            body.Should().NotContain("_links");
        }



        [TestMethod]
        public void configExclusiveCj_GetNoAccept_contentTypeIsCj()
        {
            // arrange
            ConfigureCjExclusively();

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var contentType = response.Content.Headers.ContentType.ToString();

            // assert
            contentType.Should().Contain("application/vnd.collection+json");
        }



        [TestMethod]
        public void configExclusiveCj_GetAcceptCj_contentTypeIsCj()
        {
            // arrange
            ConfigureCjExclusively();

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.collection+json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var contentType = response.Content.Headers.ContentType.ToString();

            // assert
            contentType.Should().Contain("application/vnd.collection+json");
        }



        [TestMethod]
        public void configExclusiveCj_GetAcceptJson_statusCodeIs406()
        {
            // arrange
            ConfigureCjExclusively();

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(406);
        }



        [TestMethod]
        public void configExclusiveCjAsJson_GetAcceptJson_statusCodeIs200()
        {
            // arrange
            ConfigureCjExclusively(asJson: true);

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(200);
        }



        [TestMethod]
        public void configExclusiveCjAsJson_GetAcceptJson_contentTypeIsJson()
        {
            // arrange
            ConfigureCjExclusively(asJson: true);

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var contentType = response.Content.Headers.ContentType.ToString();

            // assert
            contentType.Should().Contain("application/json"); // That's what you asked for, that's what we give you Hal IS Json after all
            // TODO: is there a way, though, to alert the client we gave more than she asked for, i.e. Content-type: application/hal+json
        }



        [TestMethod]
        public void configExclusiveCjAsJson_GetAcceptJson_bodyIsCj()
        {
            // arrange
            ConfigureCjExclusively(asJson: true);

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var body = response.Content.ReadAsStringAsync().Result;

            // assert
            body.Should().Contain("links");
            body.Should().NotContain("_links");
        }



        [TestMethod]
        public void configExclusiveCjAsJson_GetAcceptCj_contentTypeIsCj()
        {
            // arrange
            ConfigureCjExclusively(asJson: true);

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.collection+json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var contentType = response.Content.Headers.ContentType.ToString();

            // assert
            contentType.Should().Contain("application/vnd.collection+json"); // That's what you asked for, that's what we give you Hal IS Json after all
        }


        [TestMethod]
        public void configExclusiveCjAndForm_PostWwwFormUrlencoded_StatusCodeIs201()
        {
            // arrange
            ConfigureCjExclusively();
            _config.Formatters.Add(new JQueryMvcFormUrlEncodedFormatter());

            // act
            var response = _client.PostAsync(FakeBaseAddress + "/api/basic",
                new StringContent("make=Ford&model=Mustang", Encoding.ASCII, "application/x-www-form-urlencoded")).Result;

            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(201);
        }



        [TestMethod]
        public void configOnlyHal_GetNoAccept_statusCodeIs200()
        {
            // arrange
            _config.Formatters.Clear();
            _config.Formatters.Add(new HalJsonMediaTypeFormatter());

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(200);
        }



        [TestMethod]
        public void configOnlyHal_GetNoAccept_bodyIsHal()
        {
            // arrange
            _config.Formatters.Clear();
            _config.Formatters.Add(new HalJsonMediaTypeFormatter());

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var body = response.Content.ReadAsStringAsync().Result;

            // assert
            body.Should().Contain("_links");
        }



        [TestMethod]
        public void configHal_GetAcceptJson_bodyIsHal()
        {
            // arrange
            _config.Formatters.Clear();
            _config.Formatters.Add(new HalJsonMediaTypeFormatter());

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var body = response.Content.ReadAsStringAsync().Result;

            // assert
            body.Should().Contain("_links");
        }



        [TestMethod]
        public void configHal_GetAcceptJson_contentTypeIsHal()
        {
            // arrange
            _config.Formatters.Clear();
            _config.Formatters.Add(new HalJsonMediaTypeFormatter());

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var contentType = response.Content.Headers.ContentType.ToString();

            // assert
            contentType.Should().Contain("application/hal+json");
        }



        [TestMethod]
        public void configExclusiveHal_GetNoAccept_contentTypeIsHal()
        {
            // arrange
            ConfigureHalExclusively();

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var contentType = response.Content.Headers.ContentType.ToString();

            // assert
            contentType.Should().Contain("application/hal+json");
        }




        [TestMethod]
        public void configExclusiveHal_GetAcceptHal_contentTypeIsHal()
        {
            // arrange
            ConfigureHalExclusively();

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var contentType = response.Content.Headers.ContentType.ToString();

            // assert
            contentType.Should().Contain("application/hal+json");
        }



        [TestMethod]
        public void configExclusiveHal_GetAcceptJson_statusCodeIs406()
        {
            // arrange
            ConfigureHalExclusively();

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(406);
        }



        [TestMethod]
        public void configExclusiveHalAsJson_GetAcceptJson_statusCodeIs200()
        {
            // arrange
            ConfigureHalExclusively(asJson: true);

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(200);
        }



        [TestMethod]
        public void configExclusiveHalAsJson_GetAcceptJson_contentTypeIsJson()
        {
            // arrange
            ConfigureHalExclusively(asJson: true);

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var contentType = response.Content.Headers.ContentType.ToString();

            // assert
            contentType.Should().Contain("application/json"); // That's what you asked for, that's what we give you Hal IS Json after all
            // TODO: is there a way, though, to alert the client we gave more than she asked for, i.e. Content-type: application/hal+json
        }



        [TestMethod]
        public void configExclusiveHalAsJson_GetAcceptJson_bodyIsHal()
        {
            // arrange
            ConfigureHalExclusively(asJson: true);

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var body = response.Content.ReadAsStringAsync().Result;

            // assert
            body.Should().Contain("_links");
        }



        [TestMethod]
        public void configExclusiveHalAsJson_GetAcceptHal_contentTypeIsHal()
        {
            // arrange
            ConfigureHalExclusively(asJson: true);

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var contentType = response.Content.Headers.ContentType.ToString();

            // assert
            contentType.Should().Contain("application/hal+json"); // That's what you asked for, that's what we give you Hal IS Json after all
        }



        [TestMethod]
        public void configExclusiveHal_PostStringAsHal_StatusCodeIs201()
        {
            // arrange
            ConfigureHalExclusively();

            // act
            var response = _client.PostAsync(FakeBaseAddress + "/api/basic",
                new StringContent("{make: \"Ford\",model: \"Mustang\"}", Encoding.UTF8, "application/hal+json")).Result;

            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(201);
        }



        [TestMethod]
        public void configExclusiveHalAndForm_PostWwwFormUrlencoded_StatusCodeIs201()
        {
            // arrange
            ConfigureHalExclusively();
            _config.Formatters.Add(new JQueryMvcFormUrlEncodedFormatter());

            // act
            var response = _client.PostAsync(FakeBaseAddress + "/api/basic",
                new StringContent("make=Ford&model=Mustang", Encoding.ASCII, "application/x-www-form-urlencoded")).Result;

            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(201);
        }



        [TestMethod]
        public void configHalThenCj_GetNoAccept_statusCodeIs200()
        {
            // arrange
            ConfigureHalThenCj();

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(200);
        }



        [TestMethod]
        public void configHalThenCj_GetNoAccept_contentIsHal()
        {
            // arrange
            ConfigureHalThenCj();

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var body = response.Content.ReadAsStringAsync().Result;

            // assert
            body.Should().Contain("_links");
        }




        [TestMethod]
        public void configHalThenCj_GetNoAccept_contentTypeIsHal()
        {
            // arrange
            ConfigureHalThenCj();

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var contentType = response.Content.Headers.ContentType.ToString();

            // assert
            contentType.Should().Contain("application/hal+json");
            contentType.Should().NotContain("application/vnd.collection+json");
        }




        [TestMethod]
        public void configHalThenCj_GetAcceptHal_contentIsHal()
        {
            // arrange
            ConfigureHalThenCj();

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var body = response.Content.ReadAsStringAsync().Result;

            // assert
            body.Should().Contain("_links");
        }




        [TestMethod]
        public void configHalThenCj_GetAcceptHal_contentTypeIsHal()
        {
            // arrange
            ConfigureHalThenCj();

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/hal+json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var contentType = response.Content.Headers.ContentType.ToString();

            // assert
            contentType.Should().Contain("application/hal+json");
            contentType.Should().NotContain("application/vnd.collection+json");
        }




        [TestMethod]
        public void configHalThenCj_GetAcceptCj_contentIsCj()
        {
            // arrange
            ConfigureHalThenCj();

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.collection+json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var body = response.Content.ReadAsStringAsync().Result;

            // assert
            body.Should().Contain("links");
            body.Should().NotContain("_links");
        }




        [TestMethod]
        public void configHalThenCj_GetAcceptCj_contentTypeIsCj()
        {
            // arrange
            ConfigureHalThenCj();

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.collection+json"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var contentType = response.Content.Headers.ContentType.ToString();

            // assert
            contentType.Should().Contain("application/vnd.collection+json");
            contentType.Should().NotContain("application/hal+json");
        }



        [TestMethod]
        public void configExclusiveHalThenCj_GetAcceptXml_statusCodeIs406()
        {
            // arrange
            ConfigureHalThenCj();
            _config.Services.Replace(typeof(IContentNegotiator), new DefaultContentNegotiator(excludeMatchOnTypeOnly: true));

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var statusCode = (int)response.StatusCode;

            // assert
            statusCode.Should().Be(406);
        }



        [TestMethod]
        public void configHalCjXml_GetAcceptXml_ContentTypeIsXml()
        {
            // arrange
            ConfigureHalThenCj();
            _config.Formatters.Add(new XmlMediaTypeFormatter());

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));

            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var contentType = response.Content.Headers.ContentType.ToString();

            // assert
            contentType.Should().Contain("application/xml");
        }



        [TestMethod]
        public void configHalCjXml_GetAcceptXml_ContentIsXml()
        {
            // arrange
            ConfigureHalThenCj();
            _config.Formatters.Add(new XmlMediaTypeFormatter());

            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));


            // act
            var response = _client.GetAsync(FakeBaseAddress + "/api/basic/1").Result;
            var body = response.Content.ReadAsStringAsync().Result;

            // assert
            body.Should().Contain("<Name>alpha</Name>");
        }
    }
}
