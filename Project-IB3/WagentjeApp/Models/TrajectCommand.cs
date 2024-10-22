using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WagentjeApp.Models
{
    public class TrajectCommand
    {
        public string Action { get; set; }
        public int Duration { get; set; }
        public string Name { get; set; }
        public TrajectCommand(string action, int duration, string name)
        {
            Action = action;
            Duration = duration;
            Name = name;
        }
    }
}
