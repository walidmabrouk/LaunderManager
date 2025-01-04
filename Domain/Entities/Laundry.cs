using System.Reflection.PortableExecutable;

namespace LaunderWebApi.Entities
{
    public class Laundry
    {
        public int Id { get; set; } // ID principal
        public int ProprietorId { get; set; } // Liaison avec le propriétaire
        public string Name { get; set; }
        public string Address { get; set; }
        public decimal Earnings { get; set; }
        public List<Machine> Machines { get; set; } = new List<Machine>();
    }
}
