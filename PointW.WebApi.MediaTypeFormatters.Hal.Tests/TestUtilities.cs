using System.IO;
using System.Net.Http;
using System.Text;

namespace PointW.WebApi.MediaTypeFormatters.Hal.Tests
{
    public class TestUtilities
    {
        public static string FormatObject(object toFormat, HalJsonMediaTypeFormatter formatter)
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



        public static T PerformRoundTrip<T>(object o) where T : class
        {
            var formatter = new HalJsonMediaTypeFormatter();

            var json = FormatObject(o, formatter);

            var stream = GenerateStreamFromString(json);
            var content = new StreamContent(stream);

            return formatter.ReadFromStreamAsync(typeof(T), stream, content, null).Result as T;
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