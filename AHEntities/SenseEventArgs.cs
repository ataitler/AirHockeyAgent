using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHEntities
{
    public class SenseEventArgs : EventArgs
    {
        private SensePlanArg planType;
        private SenseEventType eventType;
        private DateTime timeStamp;

        public SensePlanArg PlanType
        {
            get { return planType; }
            set { planType = value; }
        }

        public SenseEventType EventType
        {
            get { return eventType; }
            set { eventType = value; }
        }

        public DateTime TimeStamp
        {
            get { return new DateTime(timeStamp.Ticks); }
        }

        public SenseEventArgs() { }

        public SenseEventArgs(SensePlanArg plan, SenseEventType type)
        {
            PlanType = plan;
            EventType = type;
            timeStamp = DateTime.Now;
        }
    }
}
