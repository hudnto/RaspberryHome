using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainServer.Messages
{
    class ReviewAlarmGroup
    {
        public ReviewAlarmGroup(bool iamathome)
        {
            AmIAtHome = iamathome;
        }

        public bool AmIAtHome { get; private set; }
    }
}
