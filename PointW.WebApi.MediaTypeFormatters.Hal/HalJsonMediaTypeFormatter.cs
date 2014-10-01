using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace PointW.WebApi.MediaTypeFormatters.Hal
{
    public class HalJsonMediaTypeFormatter : JsonMediaTypeFormatter
    {
        private readonly ResourceConverter _resourceConverter = new ResourceConverter();
        private readonly LinksConverter _linksConverter = new LinksConverter();

        public HalJsonMediaTypeFormatter()
        {
            SupportedMediaTypes.Clear(); // because we're inheriting from JsonMediaTypeFormatter, the default "application/json" is now gone
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/hal+json")); // adding this first makes it default
            // SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json")); // adding this second lets us respond with HAL to requests for just json
            SerializerSettings.Converters.Add(_resourceConverter);
            SerializerSettings.Converters.Add(_linksConverter);
            SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            SerializerSettings.ContractResolver = new HalContractResolver();
#if DEBUG
            Indent = true;
#endif
        }
    }
}
