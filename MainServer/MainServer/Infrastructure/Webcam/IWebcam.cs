using System.Net;

namespace MainServer.Infrastructure.Webcam
{
    internal interface IWebcam
    {
        string Username { get; set; }
        string Password { set; }
        IPAddress IpAddress { get; set; }
        int Port { get; set; }
        void Alarm();
        void Disalarm();
    }
}