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

		    var wc = new WebClient();
		    var url = "http://" + HubIp + "/api/" + Key + "/groups/0/action";
		    wc.UploadString(url, "PUT", content);
		}
	}
}
