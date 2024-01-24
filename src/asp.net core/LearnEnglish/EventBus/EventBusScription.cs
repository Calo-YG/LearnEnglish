using System.Text.Json.Serialization.Metadata;
using System.Text.Json;

namespace EventBus
{
    public class EventBusScription
    {
        public Dictionary<string, Type> EventTypes { get; } = [];

        public Dictionary<Type, HashSet<Type>> HandlerTypes { get; } = [];

        public JsonSerializerOptions JsonSerializerOptions { get; } = new(DefaultSerializerOptions);

        internal static readonly JsonSerializerOptions DefaultSerializerOptions = new()
        {
            TypeInfoResolver = JsonSerializer.IsReflectionEnabledByDefault ? CreateDefaultTypeResolver() : JsonTypeInfoResolver.Combine()
        };


        private static IJsonTypeInfoResolver CreateDefaultTypeResolver()
              => new DefaultJsonTypeInfoResolver();
    }
}
