using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHTacticLayer
{
    using System.Threading;
    using AHEntities;
    using AHLowlevelLayer;

    class PeriodicActionPlanningSchedulingStrategy : IActionPlanningSchedulingStrategy
    {
        private ModuleState internalState;
        private LowLevelLayer lowLevelLayer;
        private TimeSpan SceduelingPeriod;
        private ActionDirective A, lastA;
        private bool newPlanRequest;

        public PeriodicActionPlanningSchedulingStrategy(double period, ActionDirective action, LowLevelLayer lowLevel)
        {
            SceduelingPeriod = TimeSpan.FromSeconds(period);
            A = action;
            lastA = new ActionDirective(action);
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
                    lock (A.ActionDirectiveLock)
                    {
                        lastA = new ActionDirective(A);
                    }
                    // new planning of action
                    lowLevelLayer.PlanNewAction(A, true);
                }
                else
                {
                    if (DateTime.Compare(DateTime.Now, A.TimeStamp + A.Duration) < 0)
                    {
                        // replanning current action only if it's an attack
                        if (IsDirectedAttack(A))
                            lowLevelLayer.PlanNewAction(A, false);
                    }
                }

                Thread.Sleep(SceduelingPeriod);
            }
        }
         
        public override void MakePlanRequest(SenseEventType EventType)
        {
            if (EventType != SenseEventType.yWall)
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

        private bool IsDirectedAttack(ActionDirective ad)
        {
            switch (ad.Action)
            {
                case Action.ATTACK_LEFT:
                    return true;
                case Action.ATTACK_MIDDLE:
                    return true;
                case Action.ATTACK_RIGHT:
                    return true;
                case Action.DEFENSE_ATTACK:
                    return true;
                default:
                    return false;
            }
        }
    }
}
