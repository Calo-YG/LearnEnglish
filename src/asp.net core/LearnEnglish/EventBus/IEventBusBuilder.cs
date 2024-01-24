using Microsoft.Extensions.DependencyInjection;

namespace EventBus
{
    public class IEventBusBuilder
    {
        public IServiceCollection Services { get; }
    }
}
