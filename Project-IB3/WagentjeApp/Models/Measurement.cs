using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WagentjeApp.Models
{
    public class Measurement
    {
        public string Type { get; set; }
        public double Value { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
