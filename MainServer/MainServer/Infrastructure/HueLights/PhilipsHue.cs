using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Nancy.Conventions;

namespace MainServer.Infrastructure.HueLights
{
    class PhilipsHue
    {
        public static IPAddress HubIp;
        public static string Key;
        public static string SceneIn;

        public static void LightsOn()
        {
            SetLights(10);
        }

        public static void LightsOff()
        {
            SetLights(0);
        }

        private static void SetLights(int level)
        {
            string content;
            if (level == 0)
            {
                content = @"
                            {
	                            ""on"": false
                            }

                            ";
            }
            else
            {
                content = @"
                            {
	                            ""scene"": """ + SceneIn + @"""
                            }

                            ";
            }

            var data = Encoding.Default.GetBytes(content);

            var wc = WebRequest.Create("http://" + HubIp.ToString() + "/api/" + Key + "/groups/0/action");
            ((HttpWebRequest) wc).UserAgent = "MainServer.RaspberryHome(0.1)";
            wc.ContentType = "application/json";
            wc.ContentLength = data.Length;
            wc.Method = "PUT";

            var stream = wc.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();
            var webresponse = wc.GetResponse();
        }
    }
}
