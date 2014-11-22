using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using MainServer.Infrastructure;
using MainServer.Messages;
using Debug = System.Diagnostics.Debug;

namespace MainServer.Actors
{
    class MobileActor : ReceiveActor
    {
        private readonly IPAddress _myIp;
        private readonly int _retriesInSuperConnectedMode;
        private readonly bool _triggersWebcamAlarm;
        private DateTime? _enterTime = null;
        private DateTime? _leaveTime = null;
        private int _retries;

        public static Props Props(ActorElement ae, int retriesInSuperConnectedMode)
        {
            return Akka.Actor.Props.Create(() => new MobileActor(ae, retriesInSuperConnectedMode));
        }

        public MobileActor(ActorElement ae, int retriesInSuperConnectedMode)
        {
            _myIp = IPAddress.Parse(ae.Ip);
            _triggersWebcamAlarm = ae.AlarmWebCam == "Y";
            _retriesInSuperConnectedMode = retriesInSuperConnectedMode;

            Receive<Connected>(connected =>
            {
                Logger.AddEvent(Self.Path.Name + " is definitely connected :-) !");
                Become(Connected);
            });

            Receive<Pinging>(message =>
            {
                if (CheckIfConnected(Self.Path.Name))
                {
                    Become(Connected);
                }
                else
                {
                    Become(Disconnected);
                }
            });

            Receive<ReviewAlarmGroup>(@group => Sender.Tell(new ReviewAlarmGroup(false)));
        }

        private void Disconnected()
        {
            _leaveTime = DateTime.Now;
            Logger.AddEvent(Self.Path.Name + " disconnected! Has been with us for " + (_leaveTime - _enterTime).ToString());
            _enterTime = null;

            if (_triggersWebcamAlarm)
            {
                Context.Parent.Tell(new Disconnected(), Self);
            }

            Receive<Pinging>(message =>
            {
                if (CheckIfConnected(Self.Path.Name))
                {
                    Become(Connected);
                }
            });

            Receive<Connected>(connected =>
            {
                Logger.AddEvent(Self.Path.Name + " is definitely connected!");
                Become(SuperConnected);
            });

            Receive<ReviewAlarmGroup>(@group => Sender.Tell(new ReviewAlarmGroup(false)));

        }

        private void Connected()
        {
            SetupIHaveConnected();

            if (_triggersWebcamAlarm)
            {
                Context.Parent.Tell(new Connected(), Self);
            }

            Receive<PushBulletMessage>(bullet => { });

            Receive<Connected>(connected =>
            {
                Logger.AddEvent(Self.Path.Name + " is definitely connected!");
                Become(SuperConnected);
            });

            Receive<Pinging>(message =>
            {
                if (!CheckIfConnected(Self.Path.Name))
                {
                    Become(Disconnected);
                }
            });

            Receive<ReviewAlarmGroup>(@group => Sender.Tell(new ReviewAlarmGroup(_triggersWebcamAlarm)));

        }

        private void SetupIHaveConnected()
        {
            if (_enterTime == null)
            {
                _enterTime = DateTime.Now;
                StringBuilder sb = new StringBuilder(Self.Path.Name + " connected @ " + _enterTime.ToString() + "!");
                if (_leaveTime != null)
                {
                    sb.Append(" Last time we saw you around was " + _leaveTime.ToString());
                    _leaveTime = null;
                }
                Logger.AddEvent(sb.ToString());
            }
        }

        private void SuperConnected()
        {
            _retries = 0;

            SetupIHaveConnected();

            if (_triggersWebcamAlarm)
            {
                Context.Parent.Tell(new Connected(), Self);
            }

            Receive<Connected>(connected => _retries = 0);

            Receive<Pinging>(message =>
            {
                if (!CheckIfConnected(Self.Path.Name))
                {
                    _retries++;
                    Debug.WriteLine(Self.Path.Name + " - Number of retries = " + _retries.ToString(CultureInfo.InvariantCulture));
                    if (_retries >= _retriesInSuperConnectedMode)
                    {
                        Become(Connected);
                    }
                }
                else
                {
                    _retries = 0;
                }
            });

            Receive<ReviewAlarmGroup>(@group => Sender.Tell(new ReviewAlarmGroup(_triggersWebcamAlarm)));

        }


        private bool CheckIfConnected(string name)
        {
            Debug.WriteLine("Pinging " + name);
            var ping = new Ping();
            var reply = ping.Send(_myIp, 5000);
            if (reply == null || reply.Status != IPStatus.Success)
            {
                Debug.WriteLine("No pong received from " + name);
                return false;
            }
            else
            {
                Debug.WriteLine("Pong! from " + name);
                return true;
            }
        }

    }
}
