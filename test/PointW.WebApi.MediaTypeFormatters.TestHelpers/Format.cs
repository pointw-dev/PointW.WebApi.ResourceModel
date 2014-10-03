using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;

namespace PointW.WebApi.MediaTypeFormatters.TestHelpers
{
    public class Format
    {
        public static string FormatObject(object toFormat, BaseJsonMediaTypeFormatter formatter)
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



        public static TResource PerformRoundTrip<TResource>(object o, BaseJsonMediaTypeFormatter formatter) where TResource : class
        {
            var json = FormatObject(o, formatter);

            var stream = GenerateStreamFromString(json);
            var content = new StreamContent(stream);

            return formatter.ReadFromStreamAsync(typeof(TResource), stream, content, null).Result as TResource;
        }



        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
