using System;
using System.Collections.Generic;
using System.Linq;
using PointW.WebApi.ResourceModel;
using Newtonsoft.Json;

namespace PointW.WebApi.MediaTypeFormatters.Hal
{
    public class LinksConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(LinkCollection).IsAssignableFrom(objectType);
        }



        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var links = (LinkCollection) value;

            writer.WriteStartObject();

            if (links.Qualifiers.Any())
            {
                WriteCuries(writer, links.Qualifiers);
            }


            foreach (var link in links)
            {
                var rel = link.Key;
                var theLink = link.Value;

                writer.WritePropertyName(rel);
                WriteLink(writer, theLink);
            }

            writer.WriteEndObject();
        }


        
        private static void WriteLink(JsonWriter writer, Link link)
        {

            writer.WriteStartObject();

            WritePropertyIfNotNull(writer, link.Name, "name");

            writer.WritePropertyName("href");
            writer.WriteValue(link.Href);

            if (link.IsTemplated)
            {
                writer.WritePropertyName("templated");
                writer.WriteValue(true);
            }

            if (link.IsDeprecated)
            {
                writer.WritePropertyName("deprecation");
                writer.WriteValue(true);
            }

            writer.WriteEndObject();
        }



        private static void WritePropertyIfNotNull(JsonWriter writer, string propValue, string propName)
        {
            if (string.IsNullOrEmpty(propValue)) return;
            
            writer.WritePropertyName(propName);
            writer.WriteValue(propValue);
        }



        private static void WriteCuries(JsonWriter writer, IEnumerable<Link> curies)
        {
            writer.WritePropertyName("curies");
            writer.WriteStartArray();
            foreach (var curie in curies)
            {
                WriteLink(writer, curie);
            }
            writer.WriteEndArray();
        }



        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
