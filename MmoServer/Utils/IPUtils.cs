using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MmoServer.Utils
{
    public struct IPData
    {
        public string public_Ip;
        public int public_port;

        public string private_Ip;
        public int private_port;
    }

    public class IPUtils
    {
        public static async Task<IPData> GetIPData()
        {
            var data = new IPData();
            var ipEntry = await Dns.GetHostEntryAsync(Dns.GetHostName());
            using (var wc = new WebClient())
            {
                string checkWebDomain = "http://ipinfo.io/ip";
                var ipStr = await wc.DownloadStringTaskAsync(checkWebDomain);
                if (string.IsNullOrWhiteSpace(ipStr.Trim()) == false)
                {
                    data.public_Ip = ipStr.Trim();
                    data.private_Ip = ipEntry.AddressList[0].ToString();
                }
            }
            return data;
        }
    }
}
