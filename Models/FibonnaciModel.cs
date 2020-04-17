using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HAN.Demo.Models
{
    public class FibonnaciModel
    {
        public int Index { get; set; }
        public ulong Number { get; set; }

        public TimeSpan CalculationTime { get; set; }

        public DateTime GeneratedOn { get; set; } = DateTime.Now;
    }
}
