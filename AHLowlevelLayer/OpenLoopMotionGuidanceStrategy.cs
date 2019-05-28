using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AHEntities;
using Communicator;

namespace AHLowlevelLayer
{
    public class OpenLoopMotionGuidanceStrategy : IMotionGuidanceStrategy
    {
        private ModuleState internalState;
        private TrajectoryQueue commandsQueue;
        private Communicator.Communicator communicator;
        private TimeSpan Ts;

        public OpenLoopMotionGuidanceStrategy(TrajectoryQueue queue, Communicator.Communicator com, int timeCycle)
        {
            communicator = com;
            commandsQueue = queue;
            Ts = TimeSpan.FromMilliseconds(timeCycle);
            internalState = ModuleState.Inactive;
        }

        public override void MotionGuidance()
        {
            while (internalState == ModuleState.Active)
            {
                communicator.SendMessage(Command.Message, DoubleToString(commandsQueue.GetNext(QueueType.Velocity)));
                Thread.Sleep(Ts);
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
