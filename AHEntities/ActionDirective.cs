using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHEntities
{
    public class ActionDirective
    {
        private object actionLock;
        private AHEntities.Action action;
        private DateTime timeStamp;
        private TimeSpan duration;

        public object ActionDirectiveLock
        {
            get { return actionLock; }
        }

        public AHEntities.Action Action
        {
            get { return action; }
            set { action = value; }
        }

        public DateTime TimeStamp
        {
            get { return new DateTime(timeStamp.Ticks); }
            set {  timeStamp = value; }
        }

        public TimeSpan Duration
        {
            get { return new TimeSpan(duration.Ticks); }
            set { duration = value; }
        }

        public ActionDirective()
        {
            action = AHEntities.Action.LEAVE;
            timeStamp = DateTime.Now;
            duration = new TimeSpan(0, 0, 1);
            actionLock = new object();
        }

        public ActionDirective(AHEntities.Action a, DateTime t)
        {
            action = a;
            timeStamp = t;
            duration = TimeSpan.FromSeconds(0);
            actionLock = new object();
        }

        public ActionDirective(ActionDirective A2Clone)
        {
            action = A2Clone.action;
            timeStamp = A2Clone.TimeStamp;
            duration = A2Clone.Duration;
            actionLock = new object();
        }

    }
}
