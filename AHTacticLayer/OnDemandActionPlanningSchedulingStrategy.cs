using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using AHEntities;
using AHLowlevelLayer;


namespace AHTacticLayer
{
    class OnDemandActionPlanningSchedulingStrategy : IActionPlanningSchedulingStrategy
    {

        private ModuleState internalState;
        private LowLevelLayer lowLevelLayer;
        private TimeSpan SceduelingPeriod;
        private ActionDirective A;
        private bool newPlanRequest;

        public OnDemandActionPlanningSchedulingStrategy(double period, ActionDirective action, LowLevelLayer lowLevel)
        {
            SceduelingPeriod = TimeSpan.FromSeconds(period);
            A = action;
            lowLevelLayer = lowLevel;
            newPlanRequest = false;
        }

        public override void PlanningScheduler()
        {
            while (internalState == ModuleState.Active)
            {
                if (newPlanRequest)
                {
                    newPlanRequest = false;
                    lowLevelLayer.PlanNewAction(A, true);
                }

                Thread.Sleep(SceduelingPeriod);
            }
        }

        public override void MakePlanRequest(SenseEventType EventType)
        {
            newPlanRequest = true;
        }

        public override ModuleState Activate()
        {
            internalState = ModuleState.Active;
            newPlanRequest = false;
            return internalState;
        }

        public override ModuleState Deactivate()
        {
            internalState = ModuleState.Inactive;
            newPlanRequest = false;
            return internalState;
        }
    }
}
