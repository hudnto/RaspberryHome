using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MainServer.Infrastructure
{
    class ActorSection : ConfigurationSection
    {

        public const string SectionName = "ActorSet";
        private const string ActorCollectionName = "TheActors";

        [ConfigurationProperty(ActorCollectionName)]
        [ConfigurationCollection(typeof(ActorCollection), AddItemName = "add")]
        public ActorCollection ConnectionManagerEndpoints { get { return (ActorCollection)base[ActorCollectionName]; } }

    }
    public class ActorCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ActorElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ActorElement)element).ActorName;
        }
    }

    public class ActorElement : ConfigurationElement
    {
        [ConfigurationProperty("actorName", IsRequired = true)]
        public string ActorName
        {
            get { return (string)this["actorName"]; }
            set { this["actorName"] = value; }
        }

        [ConfigurationProperty("ip", IsRequired = true)]
        public string Ip
        {
            get { return (string)this["ip"]; }
            set { this["ip"] = value; }
        }

        [ConfigurationProperty("type", IsRequired = true)]
        public string ActorType
        {
            get { return (string)this["type"]; }
            set { this["type"] = value; }
        }

        [ConfigurationProperty("alarmWebCams", IsRequired = false, DefaultValue = "N")]
        public string AlarmWebCam
        {
            get { return (string)this["alarmWebCams"]; }
            set { this["alarmWebCams"] = value; }
        }

        [ConfigurationProperty("webcammodel", IsRequired = false)]
        public string WebCamModel
        {
            get { return (string)this["webcammodel"]; }
            set { this["webcammodel"] = value; }
        }

        [ConfigurationProperty("webcamusername", IsRequired = false)]
        public string WebCamUsername
        {
            get { return (string)this["webcamusername"]; }
            set { this["webcamusername"] = value; }
        }

        [ConfigurationProperty("webcampassword", IsRequired = false)]
        public string WebCamPassword
        {
            get { return (string)this["webcampassword"]; }
            set { this["webcampassword"] = value; }
        }

        [ConfigurationProperty("webcamport", IsRequired = false)]
        public string WebCamPort
        {
            get { return (string)this["webcamport"]; }
            set { this["webcamport"] = value; }
        }

        [ConfigurationProperty("pushbulletkey", IsRequired = false)]
        public string PushbulletKey
        {
            get { return (string)this["pushbulletkey"]; }
            set { this["pushbulletkey"] = value; }
        }

        [ConfigurationProperty("pushbulletDeviceToSendMessagesTo", IsRequired = false)]
        public string PushbulletDeviceToSendMessagesTo
        {
            get { return (string)this["pushbulletDeviceToSendMessagesTo"]; }
            set { this["pushbulletDeviceToSendMessagesTo"] = value; }
        }
    }
}
