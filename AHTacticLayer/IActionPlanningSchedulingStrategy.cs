using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AHEntities;

namespace AHTacticLayer
{
    public abstract class IActionPlanningSchedulingStrategy
    {
        public abstract void PlanningScheduler();

        public abstract void MakePlanRequest(SenseEventType EventType);

        public abstract ModuleState Activate();

        public abstract ModuleState Deactivate();
    }
}
