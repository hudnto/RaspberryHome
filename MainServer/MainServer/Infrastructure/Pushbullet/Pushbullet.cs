using System;
using System.Net;
using System.Text;

namespace MainServer.Infrastructure.Pushbullet
{
    class Pushbullet
    {
        private const string Host = "https://api.pushbullet.com";

        public string ApiKey { get; set; }
        public string DeviceId { get; set; }

        public Pushbullet(string apiKey, string deviceId)
        {
            ApiKey = apiKey;
            DeviceId = deviceId;
        }

        public string PushNote(string title, string body)
        {
            var wc = new WebClient();
            var authEncoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(ApiKey + ":"));
            wc.Headers[HttpRequestHeader.UserAgent] = "MainServer.RaspberryHome(0.1)";
            wc.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", authEncoded);

            //Set the headers to accept json and send form data
            wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            wc.Headers[HttpRequestHeader.Accept] = "application/json";

            var parameters = String.Format("device_id={0}&type={1}&title={2}&body={3}", DeviceId, "note", System.Uri.EscapeDataString(title), System.Uri.EscapeDataString(body));
            string result = wc.UploadString(Host + "/v2/pushes", parameters);
            return result;
        }
    }
}
