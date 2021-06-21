using CoreNet.Utils.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MmoServer.Configs
{
    public class JsonicServerConfig : JsonicObj
    {
        public int Port { get; protected set; }
        public int Max_Thread_Cnt { get; protected set; }
        public string Category { get; protected set; }
        public string Dh_iv { get; protected set; }
        public string Rsa_Private_key { get; protected set; }
        public string Rsa_Public_key { get; protected set; }
        public RSAParameters rsaParam_Private;
        public RSAParameters rsaParam_Public;

        public JsonicServerConfig(JObject _jobj, string _confPath = "") : base(_jobj)
        {
            Init(_confPath);
        }

        private void Init(string _confPath = "")
        {
            if (Rsa_Private_key == "")
            {
                var csp = new RSACryptoServiceProvider();

                if (Rsa_Public_key == "")
                {
                    var pub_param = csp.ExportParameters(false);
                    {
                        using (var sw = new StringWriter())
                        {
                            var xs = new XmlSerializer(typeof(RSAParameters));
                            xs.Serialize(sw, pub_param);
                            Rsa_Public_key = sw.ToString();
                        }
                    }
                }

                var private_param = csp.ExportParameters(true);
                {
                    using (var sw = new StringWriter())
                    {
                        var xs = new XmlSerializer(typeof(RSAParameters));
                        xs.Serialize(sw, private_param);
                        Rsa_Private_key = sw.ToString();
                    }
                }

                if (_confPath != "")
                {
                    using (var sw = new StreamWriter(_confPath, false, Encoding.UTF8))
                    {
                        var jObj = GetJObjectFromProperties();
                        string jStr = jObj.ToString();
                        sw.Write(jStr);
                        sw.Flush();
                    }
                }
            }

            using (var sr = new StringReader(Rsa_Public_key))
            {
                var xs = new XmlSerializer(typeof(RSAParameters));
                rsaParam_Public = (RSAParameters)xs.Deserialize(sr);
            }

            using (var sr = new StringReader(Rsa_Private_key))
            {
                var xs = new XmlSerializer(typeof(RSAParameters));
                rsaParam_Private = (RSAParameters)xs.Deserialize(sr);
            }
        }

    }
}
