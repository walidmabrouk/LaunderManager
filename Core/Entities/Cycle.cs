namespace LaunderWebApi.Entities
{
    public class Cycle
    {
        public int Id { get; set; } // ID principal
        public int MachineId { get; set; } // Liaison avec la machine
        public string Name { get; set; }
        public decimal Price { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
