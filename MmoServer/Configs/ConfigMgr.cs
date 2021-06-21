using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MmoServer.Configs
{
    public class ConfigMgr
    {
        public static JsonicServerConfig ServerConf { get; private set; }

        public static void Init()
        {
            try
            {
                var jsonStr = string.Empty;
                var appSettings = ConfigurationManager.AppSettings;
                var serverConfPath = appSettings.Get("ServerConfig");
                using (var sr = new StreamReader(serverConfPath, Encoding.UTF8))
                {
                    jsonStr = sr.ReadToEnd();
                }
                var jObj = JObject.Parse(jsonStr);
                ServerConf = new JsonicServerConfig(jObj, jsonStr);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
