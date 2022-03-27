using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTP_Client.Helpers
{
    public static class Utility
    {
        public static string DecodeHex(string txt)
        {
            StringBuilder result = new StringBuilder();
            foreach (byte c in txt)
            {
                result.Append(c.ToString("x2"));
            }
            return result.ToString();
        }
    }
}
