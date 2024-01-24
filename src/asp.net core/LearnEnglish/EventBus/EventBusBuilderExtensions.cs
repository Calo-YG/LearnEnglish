using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace EventBus
{
    public static class EventBusBuilderExtensions
    {

        public static IEventBusBuilder ConfigureJsonOptions(this IEventBusBuilder eventBusBuilder, Action<JsonSerializerOptions> configure)
        {
            eventBusBuilder.Services.Configure<EventBusScription>(o =>
            {
                configure(o.JsonSerializerOptions);
            });

            return eventBusBuilder;
        }

        public static IEventBusBuilder AddSubscription<T, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TH>(this IEventBusBuilder eventBusBuilder)
            where T : IntegrationEvent
            where TH : class, IIntegrationEventHandler<T>
        {

            eventBusBuilder.Services.AddTransient<TH>();

            eventBusBuilder.Services.Configure<EventBusScription>(o =>
            {
                o.EventTypes[typeof(T).Name] = typeof(T);

                // Handle the case where the same handler is registered twice for the same event type
                if (o.HandlerTypes.TryGetValue(typeof(T), out var handlerTypes))
                {
                    if (!handlerTypes.Add(typeof(TH)))
                    {
                        throw new InvalidOperationException($"Handler Type {typeof(TH).GetGenericTypeName()} already registered for '{typeof(T)}'");
                    }
                }
                else
                {
                    o.HandlerTypes[typeof(T)] = [typeof(TH)];
                }
            });

            return eventBusBuilder;
        }
    }
}
