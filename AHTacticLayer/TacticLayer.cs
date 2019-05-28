using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using AHEntities;
using AHLowlevelLayer;

namespace AHTacticLayer
{
    public class TacticLayer
    {
        private WorldModel WM;
        private LowLevelLayer lowLevelLayer;
        private Logger.Logger mLogger;
        private ActionDirective currentActionDirective;
        private SenseEventType currentEvent;
        //private AHEntities.Action action;
        private ModuleState internalState;
        private IActionPlanningSchedulingStrategy PlanningScheduler;
        private Thread Scheduler;

        private IActionSelectionStrategy actionSelectionStrategy;
        
        public TacticLayer(WorldModel worldModel, LowLevelLayer lowLevel)
        {
            actionSelectionStrategy = new FuzzyActionSelectionStrategy(worldModel);
            WM = worldModel;
            lowLevelLayer = lowLevel;
            mLogger = Logger.Logger.Instance;
            internalState = ModuleState.Inactive;
            Hashtable consts = WM.GetConstants();
            currentActionDirective = new ActionDirective(AHEntities.Action.LEAVE, DateTime.Now);
            //PlanningScheduler = new OnDemandActionPlanningSchedulingStrategy(0.02, currentActionDirective, lowLevel);
            PlanningScheduler = new PeriodicActionPlanningSchedulingStrategy((double)consts["PlanPeriod"], currentActionDirective, lowLevel);
        }

        public void StartNewPlan(object sender, EventArgs e)
        {
            ActionDirective tempA;
            SenseEventArgs senseE = (SenseEventArgs)e;
            Dictionary<string, double> gameState = WM.GetPhysicalState();
            Point puckV = new Point(gameState["PuckVx"], gameState["PuckVy"]);

            if ((senseE.EventType == SenseEventType.yWall) && (puckV.X > 0))
            {
                // no replan (prepare action)
                currentEvent = senseE.EventType;
                mLogger.AddLogMessage("Tactics: No New Action Required: " + currentActionDirective.Action.ToString());
            }
            else
            {
                //new plan
                if ((currentEvent == senseE.EventType) && 
                    ((currentEvent == SenseEventType.StuckPlayer) || (currentEvent == SenseEventType.StuckAgent)))
                {
                    // no replan
                    mLogger.AddLogMessage("Tactics: No New Action Required: " + currentActionDirective.Action.ToString());
                }
                else if ((senseE.EventType == SenseEventType.yWall) && (puckV.X < 0))
                {
                    // need to replan current action
                    currentEvent = senseE.EventType;
                    mLogger.AddLogMessage("Tactics: Refining Old Action: " + currentActionDirective.Action.ToString());
                    PlanningScheduler.MakePlanRequest(senseE.EventType);
                }
                else
                {
                    currentEvent = senseE.EventType;
                    tempA = actionSelectionStrategy.SelectAction(senseE.EventType);
                    //if (tempA.Action != currentActionDirective.Action)
                    //{
                        lock (currentActionDirective.ActionDirectiveLock)
                        {
                            currentActionDirective.Action = tempA.Action;
                            currentActionDirective.TimeStamp = senseE.TimeStamp;
                            currentActionDirective.Duration = tempA.Duration - (DateTime.Now - senseE.TimeStamp);
                        }
                        // log action - new action
                        mLogger.AddLogMessage("Tactics: New Action Selected: " + currentActionDirective.Action.ToString());
                        PlanningScheduler.MakePlanRequest(senseE.EventType);
                    /*}
                    else
                    {
                        mLogger.AddLogMessage("Tactics: Old Action Selected: " + currentActionDirective.Action.ToString());
                        PlanningScheduler.MakePlanRequest(senseE.EventType);
                    }*/
                }
            }


            /*
            // new plan required
            if ((senseE.EventType == SenseEventType.AgentCollision) || (senseE.EventType == SenseEventType.OpponentCollision) ||
                (senseE.EventType == SenseEventType.xWall) || (senseE.EventType == SenseEventType.yWall) && (puckV.X < 0)) 
            {
                tempA = actionSelectionStrategy.SelectAction(senseE.EventType);
                if (tempA.Action != currentActionDirective.Action)
                {
                    lock (currentActionDirective.ActionDirectiveLock)
                    {
                        currentActionDirective.Action = tempA.Action;
                        currentActionDirective.TimeStamp = senseE.TimeStamp;
                        currentActionDirective.Duration = tempA.Duration - (DateTime.Now - senseE.TimeStamp);
                    }
                    // log action - new action
                    mLogger.AddLogMessage("Tactics: New Action Selected: " + currentActionDirective.Action.ToString());
                    PlanningScheduler.MakePlanRequest(senseE.EventType);
                }
                else 
                {
                    mLogger.AddLogMessage("Tactics: Old Action Selected: " + currentActionDirective.Action.ToString());
                    PlanningScheduler.MakePlanRequest(senseE.EventType);
                }
            }
            else 
            {
                // log action
                mLogger.AddLogMessage("Tactics: Refinement Of Active Action: " + currentActionDirective.Action.ToString());
                PlanningScheduler.MakePlanRequest(senseE.EventType);
            }*/




            /*
            if (senseE.PlanType == SensePlanArg.Plan)
            {
                tempA = actionSelectionStrategy.SelectAction(senseE.EventType);
                // construct the action directive object
                if (tempA.Action != currentActionDirective.Action)
                {
                    lock (currentActionDirective.ActionDirectiveLock)
                    {
                        currentActionDirective.Action = tempA.Action;
                        currentActionDirective.TimeStamp = senseE.TimeStamp;
                        currentActionDirective.Duration = tempA.Duration - (DateTime.Now - senseE.TimeStamp);
                    }
                    // log action - new action
                    mLogger.AddLogMessage("Tactics: New Action Selected: " + currentActionDirective.Action.ToString());
                }
                else
                {
                    if ((currentActionDirective.Action != AHEntities.Action.PREPARE))
                    {     
                        // log action - refinement of current action.
                        mLogger.AddLogMessage("Tactics: New (reselection) Action Selected: " + currentActionDirective.Action.ToString());
                    }
                }
                PlanningScheduler.MakePlanRequest(senseE.EventType);
            }
            else
            {
                // current directive is still valid, need to schedule replanning
                // log action
                mLogger.AddLogMessage("Tactics: Refinement Of Active Action: " + currentActionDirective.Action.ToString());
                PlanningScheduler.MakePlanRequest(senseE.EventType);
            }*/
        }
         
        public ModuleState Start()
        {
            currentActionDirective.Action = AHEntities.Action.LEAVE;
            currentEvent = SenseEventType.NoEvent;
            currentActionDirective.TimeStamp = DateTime.Now;
            Scheduler = new Thread(new ThreadStart(PlanningScheduler.PlanningScheduler));
            Scheduler.Start();
            if (internalState == ModuleState.Inactive)
            {
                internalState = ModuleState.Active;
                PlanningScheduler.Activate();
            }
            return internalState;
        }

        public ModuleState Stop()
        {
            if (internalState == ModuleState.Active)
            {
                internalState = ModuleState.Inactive;
                PlanningScheduler.Deactivate();
                Scheduler.Abort();
            }
            return internalState;
        }

    }
}
