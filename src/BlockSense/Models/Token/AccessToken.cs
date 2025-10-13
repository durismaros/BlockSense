using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense.Models.Token
{
    public class AccessToken
    {
        public string? Data { get; set; }
        public int ExpiresIn { get; set; }
    }
}
