using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace LaunderWebApi.Entities
{
    public class Laundry
    {
        public int Id { get; set; }
        public int ProprietorId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public double Earnings { get; set; }
        public List<Machine> Machines { get; set; }
    }
}
