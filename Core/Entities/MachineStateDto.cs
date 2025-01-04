using System.Text.Json.Serialization;

public class MachineStateDto : BaseMessageDto
{
    [JsonPropertyName("machineId")]
    public int MachineId { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("price")]
    public decimal? Price { get; set; }
}
