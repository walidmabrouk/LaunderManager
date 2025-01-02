namespace LaunderWebApi.Entities
{
    public class Machine
    {
        public int Id { get; set; } // ID principal
        public int LaundryId { get; set; } // Liaison avec la laverie
        public string SerialNumber { get; set; }
        public string Type { get; set; }
        public string State { get; set; }
        public decimal Earnings { get; set; }
        public List<Cycle> Cycles { get; set; } = new List<Cycle>();
    }
}
