using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using MainServer.Infrastructure;
using MainServer.Messages;
using Debug = System.Diagnostics.Debug;

namespace MainServer.Actors
{
    class WebcamParentActor : ReceiveActor
    {
        public WebcamParentActor(IEnumerable<ActorElement> listActors)
        {
            foreach (var element in listActors)
            {
                var actor = Context.ActorOf(WebCamActor.Props(element), element.ActorName);
                Logger.AddEvent("Added actor - " + actor.Path);
            }

            Receive<Connected>(c =>
            {
                foreach (var actor in Context.GetChildren())
                {
                    actor.Tell(c, Self);
                }
            });

            Receive<Disconnected>(c =>
            {
                foreach (var actor in Context.GetChildren())
                {
                    actor.Tell(c, Self);
                }
            });

        }

        public static Props Props(List<ActorElement> listActors)
        {
            return Akka.Actor.Props.Create(() => new WebcamParentActor(listActors));
        }
    }
}
