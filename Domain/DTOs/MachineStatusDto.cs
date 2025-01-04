// Application/DTOs/MachineStatusDto.cs
using System.Text.Json.Serialization;

namespace LaunderManagerWebApi.Domain.DTOs
{
    public class MachineStatusDto : BaseMessageDto
    {
        [JsonPropertyName("machineId")]
        public int MachineId { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("price")]
        public decimal? Price { get; set; }
    }
}
