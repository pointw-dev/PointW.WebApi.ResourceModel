using System;
using System.Linq;
using PointW.WebApi.ResourceModel;
using Newtonsoft.Json;

namespace PointW.WebApi.MediaTypeFormatters.CollectionJson
{
    public class LinksConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(LinkCollection).IsAssignableFrom(objectType);
        }



        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var links = (LinkCollection)value;

            writer.WriteStartArray();

            foreach (var link in links)
            {
                var rel = ExpandRel(link.Key, links);
                var theLink = link.Value;
                writer.WriteStartObject();

                writer.WritePropertyName("rel");
                writer.WriteValue(rel);

                WriteLink(writer, theLink);

                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }



        private static string ExpandRel(string rel, LinkCollection links)
        {
            if (!links.Qualifiers.Any() || !rel.Contains((":"))) return rel;

            var prefix = rel.Split(':')[0];
            var suffix = rel.Split(':')[1];

            var qualifier = links.Qualifiers.FirstOrDefault(q => q.Name == prefix);

            if (qualifier == null) return rel;

            if (!qualifier.IsTemplated) return qualifier.Href + suffix;

            return qualifier.Href.Replace("{rel}", suffix);
        }



        private static void WriteLink(JsonWriter writer, Link link)
        {
            WritePropertyIfNotNull(writer, link.Name, "name");

            writer.WritePropertyName("href");
            writer.WriteValue(link.Href);

            if (link.IsTemplated)
            {
                // TODO: not supported by Collection+JSON, either find a way to pop'l the template or skip it, I guess
                // or maybe form the template: object!!!!??
            }

            if (link.IsDeprecated)
            {
                // TODO: not supported by Collection+JSON, maybe some way to reflect it, else ignore deprecation
            }
        }



        private static void WritePropertyIfNotNull(JsonWriter writer, string propValue, string propName)
        {
            if (string.IsNullOrEmpty(propValue)) return;

            writer.WritePropertyName(propName);
            writer.WriteValue(propValue);
        }



        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
