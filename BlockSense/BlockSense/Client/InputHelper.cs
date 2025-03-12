using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockSense
{
    class InputHelper
    {
        public static bool Check(params string[] input)
        {
            foreach (string param in input)
            {
                if (string.IsNullOrEmpty(param))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
