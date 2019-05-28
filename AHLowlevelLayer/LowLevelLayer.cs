using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AHEntities;
using System.Threading;
using Communicator;
using System.Diagnostics;

namespace AHLowlevelLayer
{
    public class LowLevelLayer
    {
        private Thread Guidance;
        private IMotionGuidanceStrategy motionGuidanceStrategy;
        private IActionPlanningStrategy actionPlanningStrategy;
        private ITrajectoryPlanningStrategy trajectoryPlanningStrategy;
        private Communicator.Communicator communicator;
        private WorldModel WM;
        private ModuleState internalState;
        private short DOF;
        private double Ts;
        private PointParams bounderyConditions;
        private Logger.Logger mLogger;
        Stopwatch planTime;
        private Dictionary<string, double> state;
        private double maxTime;

        private TrajectoryQueue commandsQueue;
        
        public LowLevelLayer(Communicator.Communicator com, WorldModel worldModel, double timeStep, short degrees)
        {
            internalState = ModuleState.Inactive;
            communicator = com;
            WM = worldModel;
            DOF = degrees;
            Ts = timeStep;
            mLogger = Logger.Logger.Instance;
            planTime = new Stopwatch();
            planTime.Reset();
            Hashtable consts = WM.GetConstants();
            maxTime = (double)consts["MoveInterval"];
            commandsQueue = new TrajectoryQueue(DOF,new double[2] {-1000,0});
            bounderyConditions = new PointParams();

            //motionGuidanceStrategy = new OpenLoopMotionGuidanceStrategy(commandsQueue, com, Ts);
            motionGuidanceStrategy = new PDMotionGuidanceStrategy(commandsQueue, com, Ts, WM);
            actionPlanningStrategy = new SimpleLinesActionPlanningStrategy(WM);
            trajectoryPlanningStrategy = new PolynomialTrajectoryPlanningStrategy(Ts, maxTime);

            // dummy planning to warm up the containers
            #region Warmup
            /*
            PointParams init = new PointParams();
            init.AddParameters(new Point(0, 0));
            init.AddParameters(new Point(0, 0));
            init.T = DateTime.Now;
            //init.T = 0;
            PointParams final = new PointParams();
            final.AddParameters(new Point(0, 0));
            final.AddParameters(new Point(0, 0));
            final.T = DateTime.Now + TimeSpan.FromSeconds(0.2);
            state = WM.GetPhysicalState();
            actionPlanningStrategy.ActionPlanning(AHEntities.Action.DEFENSE_ATTACK, true);
            trajectoryPlanningStrategy.TrajectoryPlanning(init, final);
            */
            #endregion Warmup

        }

        public void PlanNewAction(ActionDirective A, bool isNewPlanning)
        {
            planTime.Restart();

            // target point parameters (for the whole motion)
            //if (isNewPlanning)
                //bounderyConditions = actionPlanningStrategy.ActionPlanning(A, isNewPlanning);
            bounderyConditions = actionPlanningStrategy.ActionPlanning(A, true);
            
            if (bounderyConditions == null)
                return;

            state = WM.GetPhysicalState();
            Point agentP = new Point(state["AgentX"], state["AgentY"]);
            Point agentV = new Point(state["AgentVx"], state["AgentVy"]);
            mLogger.AddLogMessage("LowLevel: action planned: agent currently at: " + agentP.ToString() + " and: " + agentV.ToString() + ", target: " + bounderyConditions.ToString());

            // initial movement parameters
            PointParams initialConditions = new PointParams();
            initialConditions.AddParameters(agentP);
            initialConditions.AddParameters(agentV);
            initialConditions.T = DateTime.Now;

            // new trajectory generation
            double time = (bounderyConditions.T - initialConditions.T).TotalSeconds;
            double[][,] newTrajectory = null;
            if ((isNewPlanning) || ((time < maxTime) && (time > 0)))
                newTrajectory = trajectoryPlanningStrategy.TrajectoryPlanning(initialConditions, bounderyConditions);

            if (newTrajectory != null)
            {
                mLogger.AddLogMessage("LowLevel: New Trajectory Designed, Length: " + newTrajectory[0].LongLength.ToString() + " time: " + time.ToString());
                commandsQueue.Replace(QueueType.Position, newTrajectory[0]);
                // output the whole trajectory to log
                //mLogger.AddLogMessage("LowLevel: Trajectory: " + commandsQueue.PositionToString());
                commandsQueue.Replace(QueueType.Velocity, newTrajectory[1]);
            }
            planTime.Stop();
            mLogger.AddLogMessage("LowLevel: Planning time was: " + planTime.Elapsed.TotalSeconds.ToString() + " Seconds");
        }

        public ModuleState Start()
        {
            // clear all old commands
            commandsQueue.Clear();
            commandsQueue.InitPosition(new double[2] { -1000, 0 });
            if (bounderyConditions == null)
                bounderyConditions = new PointParams();
            else
                bounderyConditions.Clear();

            if (internalState == ModuleState.Inactive)
            {
                Guidance = new Thread(new ThreadStart(motionGuidanceStrategy.MotionGuidance));
                internalState = ModuleState.Active;
                motionGuidanceStrategy.Activate();
                Guidance.Start();
            }
            return internalState;
        }

        public ModuleState Stop()
        {
            if (internalState == ModuleState.Active)
            {
                internalState = ModuleState.Inactive;
                motionGuidanceStrategy.Deactivate();
                Guidance.Abort();
            }
            return internalState;
        }

    }
}
