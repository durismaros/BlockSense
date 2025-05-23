using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Models
{
    public class ServerStatusModel
    {
        public string? Status { get; set; }
        public string? DbStatus { get; set; }
        public string? TimeStamp { get; set; }
    }
}
