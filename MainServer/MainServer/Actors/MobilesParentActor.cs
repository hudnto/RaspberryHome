using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Event;
using MainServer.Infrastructure;
using MainServer.Messages;
using Debug = System.Diagnostics.Debug;

namespace MainServer.Actors
{
    class MobilesParentActor : ReceiveActor
    { 
        public MobilesParentActor(IEnumerable<ActorElement> listActors, int retriesInSuperConnectedMode)
        {

            foreach (var element in listActors)
            {
                var actor = Context.ActorOf(MobileActor.Props(element, retriesInSuperConnectedMode), element.ActorName);
                Logger.AddEvent("Added actor - " + actor.Path);
            }

            Receive<Pinging>(pinging =>
            {
                Debug.WriteLine("Pinging all mobile devices...");
                foreach (var actor in Context.GetChildren())
                {
                    actor.Tell(pinging, Self);
                }
            });

            Receive<ReviewAlarmGroup>(@group =>
            {
                var setAlarm = true;
                var lt = Context.GetChildren().Select(actor => actor.Ask<ReviewAlarmGroup>(new ReviewAlarmGroup(false)));

                foreach (var review in lt)
                {
                    review.Wait();
                    var rw = review.Result;
                    if (rw.AmIAtHome)
                        setAlarm = false;
                }

                if (setAlarm)
                {
                    Debug.WriteLine("We need to set the alarm ON...");
                    Context.ActorSelection("/user/webcams").Tell(new Connected());
                }
                else
                {
                    Debug.WriteLine("We need to set the alarm OFF...");
                    Context.ActorSelection("/user/webcams").Tell(new Disconnected());                    
                }
            });

        }

        public static Props Props(List<ActorElement> listActors, int retriesInSuperConnectedMode)
        {
            return Akka.Actor.Props.Create(() => new MobilesParentActor(listActors, retriesInSuperConnectedMode));
        }
    }
}
