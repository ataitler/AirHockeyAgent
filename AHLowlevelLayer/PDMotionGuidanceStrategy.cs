using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using AHEntities;
using System.Diagnostics;

namespace AHLowlevelLayer
{
    class PDMotionGuidanceStrategy : IMotionGuidanceStrategy
    {
        private ModuleState internalState;
        private TrajectoryQueue commandsQueue;
        private Communicator.Communicator communicator;
        private TimeSpan Ts;
        private WorldModel WM;
        private TrajectoryQueue puckNominalTrajectory;
        double Kp, Ki, a, b;
        Dictionary<string, double> physicalState;

        public PDMotionGuidanceStrategy(TrajectoryQueue queue, Communicator.Communicator com, double timeCycle, WorldModel worldModel)
        {
            communicator = com;
            commandsQueue = queue;
            Ts = TimeSpan.FromSeconds(timeCycle);
            internalState = ModuleState.Inactive;
            WM = worldModel;
            puckNominalTrajectory = WM.GetPuckTrajectory();
            Kp = 0.5;
            //Kp = 0.8;
            Ki = 22.125;
            //Ki = 10;
            double T = Ts.TotalSeconds;
            a = (2 * Ki + T * Kp) / 2;
            b = (2 * Ki - T * Kp) / 2;
        }

        public override void MotionGuidance()
        {
            double[] puckPnom, puckVnom;
            double[] malletPnom, malletVnom;
            double deltaP0x, deltaP0y, deltaPx, deltaPy;
            double ux, uy, ux_prev, uy_prev;
            double ex_prev, ey_prev, ex, ey;
            ex_prev = 0;
            ey_prev = 0;
            ux_prev = 0;
            uy_prev = 0;
            Stopwatch planTime = new Stopwatch();

            while (internalState == ModuleState.Active)
            {
                planTime.Restart();
                puckPnom = puckNominalTrajectory.GetNext(QueueType.Position);
                puckVnom = puckNominalTrajectory.GetNext(QueueType.Velocity);
                malletPnom = commandsQueue.GetNext(QueueType.Position);
                malletVnom = commandsQueue.GetNext(QueueType.Velocity);

                physicalState = WM.GetPhysicalState();

                deltaP0x = malletPnom[0] - puckPnom[0];
                deltaP0y = malletPnom[1] - puckPnom[1];
                deltaPx = physicalState["AgentX"] - physicalState["PuckX"];
                deltaPy = physicalState["AgentY"] - physicalState["PuckY"];

                ex = deltaP0x - deltaPx;
                ey = deltaP0y - deltaPy;

                ux = a * ex + b * ex_prev - ux_prev;
                uy = a * ey + b * ey_prev - uy_prev;

                communicator.SendMessage(Command.Message, DoubleToString(new double[2] {ux, uy}));

                ex_prev = ex;
                ey_prev = ey;
                ux_prev = ux;
                uy_prev = uy;
                planTime.Stop();
                
                if ((Ts - planTime.Elapsed).TotalMilliseconds > 0 )
                    Thread.Sleep(Ts - planTime.Elapsed);
            }
        }

        public override ModuleState Activate()
        {
            internalState = ModuleState.Active;
            return internalState;
        }

        public override ModuleState Deactivate()
        {
            internalState = ModuleState.Inactive;
            return internalState;
        }

        private string DoubleToString(double[] data)
        {
            string s = String.Join(",", data.Select(p => p.ToString()).ToArray());
            return s;
        }

    }
}
