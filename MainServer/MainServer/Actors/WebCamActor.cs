using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using MainServer.Infrastructure;
using MainServer.Infrastructure.HueLights;
using MainServer.Infrastructure.Pushbullet;
using MainServer.Infrastructure.Webcam;
using MainServer.Messages;

namespace MainServer.Actors
{
    class WebCamActor : ReceiveActor
    {
        private readonly IWebcam _physicalWebcam;
        private readonly Pushbullet _pushBullet;
        private bool _firstTime = true;

        public static Props Props(ActorElement ae)
        {
            return Akka.Actor.Props.Create(() => new WebCamActor(ae));
        }

        public WebCamActor(ActorElement ae)
        {
            IPAddress ip = IPAddress.Parse(ae.Ip);

            var type = Type.GetType(ae.WebCamModel);
            if (type == null)
            {
                throw new ApplicationException("The webcam model " + ae.WebCamModel + " cannot be found in the list of available supported web cams. Please review the config file.");
            }
            _physicalWebcam = (IWebcam)Activator.CreateInstance(type,ae.WebCamUsername, ae.WebCamPassword, ip, Int32.Parse(ae.WebCamPort));

            _pushBullet = new Pushbullet(ae.PushbulletKey, ae.PushbulletDeviceToSendMessagesTo);

            Become(Disalarmed);
        }

        private void Alarmed()
        {
            Logger.AddEvent(Self.Path.Name + " alarmed!");
            _physicalWebcam.Alarm();
            PhilipsHue.LightsOff();
            _pushBullet.PushNote("Webcam from home", "The webcam has been alarmed.");

            Receive<Disconnected>(connected => Become(Disalarmed));

        }

        private void Disalarmed()
        {
            if (!_firstTime)
            {
                Logger.AddEvent(Self.Path.Name + " disalarmed!");
                _physicalWebcam.Disalarm();
                PhilipsHue.LightsOn();
            }
            else
            {
                _firstTime = false;
            }
            Receive<Connected>(connected => Become(Alarmed));
        }
    }
}
