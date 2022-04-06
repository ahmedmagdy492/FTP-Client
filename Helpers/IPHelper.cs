using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FTP_Client.Helpers
{
    public class IPHelper
    {
        public string GetMyIPAddress()
        {
            string hostName = Dns.GetHostName();

            return Dns.GetHostAddresses(hostName).FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).ToString();
        }
    }
}
