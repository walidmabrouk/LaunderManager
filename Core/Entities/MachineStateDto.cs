using System.Text.Json.Serialization;

public class MachineStateDto
{
    [JsonPropertyName("machineId")]  // Permet de gérer le nom du champ JSON en minuscule
    public int MachineId { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }
}
