using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using PointW.WebApi.ResourceModel;

namespace PointW.WebApi.MediaTypeFormatters.CollectionJson
{
    public class ResourceConverter : JsonConverter
    {
        const string StreamingContextResourceConverterToken = "collection+json";
        const StreamingContextStates StreamingContextResourceConverterState = StreamingContextStates.Other;


        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IResource).IsAssignableFrom(objectType);
        }



        public static bool IsResourceConverterContext(StreamingContext context)
        {
            return context.Context is string &&
                   (string)context.Context == StreamingContextResourceConverterToken &&
                   context.State == StreamingContextResourceConverterState;
        }



        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var resource = (IResource)value;

            var saveContext = serializer.Context;
            serializer.Context = GetResourceConverterContext();
            serializer.Converters.Remove(this);
            serializer.Serialize(writer, resource);
            serializer.Converters.Add(this);
            serializer.Context = saveContext;
        }

        // TODO: need some refactoring here with HAL resource converter
        private static StreamingContext GetResourceConverterContext()
        {
            return new StreamingContext(StreamingContextResourceConverterState, StreamingContextResourceConverterToken);
        }


    }
}
