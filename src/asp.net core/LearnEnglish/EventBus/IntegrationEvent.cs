using Masa.BuildingBlocks.Data;
using System.Text.Json.Serialization;

namespace EventBus
{
    public record class IntegrationEvent
    {
        public IntegrationEvent()
        {
            Id = IdGeneratorFactory.SequentialGuidGenerator.NewId();
            CreationDate = DateTime.Now;
        }

        [JsonInclude]
        public Guid Id { get; set; }

        [JsonInclude]
        public DateTime CreationDate { get; set; }
    }
}
