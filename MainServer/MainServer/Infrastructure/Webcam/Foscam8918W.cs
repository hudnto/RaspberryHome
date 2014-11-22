using System;
using System.Net;
using System.Text;

namespace MainServer.Infrastructure.Webcam
{
    class Foscam8918W : IWebcam
    {
        private const string urlFormat = "http://{0}:{1}@{2}:{3}/set_alarm.cgi?motion_armed={4}&motion_sensitivity={5}";
        public string Username { get; set; }
        public string Password { private get; set; }
        public IPAddress IpAddress { get; set; }
        public int Port { get; set; }

        public Foscam8918W(string username, string password, IPAddress ipAddress, int port)
        {
            Username = username;
            Password = password;
            IpAddress = ipAddress;
            Port = port;
        }

        public void Alarm()
        {
            Send(1, 4);
        }

        public void Disalarm()
        {
            Send(0, 4);
        }

        private void Send(int armedFlag, int sensitivity)
        {
            var url = string.Format(urlFormat, Username, Password, IpAddress.ToString(), Port, armedFlag, sensitivity);
            var authInfo = Username + ":" + Password;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));

            using (var client = new WebClient())
            {
                client.Headers["Authorization"] = "Basic " + authInfo;
                var result = client.DownloadString(url);
            }

        }
    }
}
