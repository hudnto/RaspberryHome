using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Akka;
using Akka.Actor;
using Akka.Util.Internal;
using MainServer.Actors;
using MainServer.Infrastructure;
using MainServer.Infrastructure.HueLights;
using MainServer.Messages;
using Nancy.Hosting.Self;
using HttpStatusCode = Nancy.HttpStatusCode;

namespace MainServer
{
    public class Program : Nancy.NancyModule
    {
        //TODO: Add dependency injection
        //TODO: Static classes with dependency injection container and move away things from Main...

        private static ActorSystem _system;
        static readonly ManualResetEvent QuitEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            Console.CancelKeyPress += (sender, eArgs) =>
            {
                QuitEvent.Set();
                eArgs.Cancel = true;
            };

            var hostConfigs = new HostConfiguration()
            {
                UrlReservations = new UrlReservations() { CreateAutomatically = true }
            };

            var appSettings = ConfigurationManager.AppSettings;

            int secondsForPing = Int32.Parse(appSettings["pingFrequencyInSeconds"]);
            int retriesInSuperConnectedMode = Int32.Parse(appSettings["retriesInSuperConnectedMode"]);

            PhilipsHue.HubIp = IPAddress.Parse(appSettings["huelightsHubIP"]);
            PhilipsHue.Key = appSettings["huelightsKey"];
            PhilipsHue.SceneIn = appSettings["huelightsscenein"];

            //var urisForNancy = GetUriParams(29005);

            using (var host = new NancyHost(hostConfigs, new Uri(appSettings["NancyURI"])))
            {
                host.Start();

                _system = ActorSystem.Create("home-automation-system");

                var connectionManagerDataSection = ConfigurationManager.GetSection(ActorSection.SectionName) as ActorSection;
                if (connectionManagerDataSection != null)
                {
                    var listMobileDevices = new List<ActorElement>();
                    var listWebcamDevices = new List<ActorElement>();
                    
                    foreach (ActorElement element in connectionManagerDataSection.ConnectionManagerEndpoints)
                    { 
                        switch (element.ActorType)
                        {
                            case "MobileActor":
                                listMobileDevices.Add(element);
                                break;

                            case "WebCamActor":
                                listWebcamDevices.Add(element);
                                break;
                        }
                    }
                    _system.ActorOf(MobilesParentActor.Props(listMobileDevices, retriesInSuperConnectedMode), "mobiles");
                    _system.ActorOf(WebcamParentActor.Props(listWebcamDevices), "webcams");
                }

                //_system.Scheduler.Schedule(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(secondsForPing), () => _system.ActorSelection("/user/mobiles/*").Tell(new Pinging()));
                _system.Scheduler.Schedule(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(secondsForPing), () => _system.ActorSelection("/user/mobiles").Tell(new Pinging()));
                _system.Scheduler.Schedule(TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(secondsForPing), () => _system.ActorSelection("/user/mobiles").Tell(new ReviewAlarmGroup(false)));

                Console.WriteLine("Your application is running on " + appSettings["NancyURI"]);
                Console.WriteLine("Press CTRL-C to close the host.");
                QuitEvent.WaitOne();
                Console.WriteLine("Shutting down system...");
                // Shutdown the system...
                _system.Shutdown();
            }
        }

        //private static Uri[] GetUriParams(int port)
        //{
        //    var uriParams = new List<Uri>();
        //    string hostName = Dns.GetHostName();

        //    // Host name URI
        //    string hostNameUri = string.Format("http://{0}:{1}", Dns.GetHostName(), port);
        //    uriParams.Add(new Uri(hostNameUri));

        //    // Host address URI(s)
        //    var hostEntry = Dns.GetHostEntry(hostName);
        //    foreach (var ipAddress in hostEntry.AddressList)
        //    {
        //        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)  // IPv4 addresses only
        //        {
        //            var addrBytes = ipAddress.GetAddressBytes();
        //            string hostAddressUri = string.Format("http://{0}.{1}.{2}.{3}:{4}", addrBytes[0], addrBytes[1], addrBytes[2], addrBytes[3], port);
        //            uriParams.Add(new Uri(hostAddressUri));
        //        }
        //    }

        //    // Localhost URI
        //    uriParams.Add(new Uri(string.Format("http://localhost:{0}", port)));
        //    return uriParams.ToArray();
        //}

        public Program()
        {
            Get["/ping/{devicetype}/{id}"] = ping =>
            {
                var id = ping.id;
                var devicetype = ping.devicetype;
                var actor = _system.ActorSelection("/user/" + devicetype + "/" + id);
                string s = this.Request.UserHostAddress;
                actor.Tell(new Connected());
                return HttpStatusCode.NoContent;
            };
        }
    }
}
