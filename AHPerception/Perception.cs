using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Communicator;
using AHEntities;

namespace AHPerception
{
    public class Perception : IPerception
    {
        public event EventHandler OnEstimationUpdate;
        public WorldModel WM;
        private ModuleState internalState;
        private IEstimationStrategy EstimatorStrategy;
        private Logger.Logger mLogger;

        public Perception(WorldModel model)
        {
            WM = model;
            internalState = ModuleState.Inactive;
            EstimatorStrategy = new SimpleEstimationStrategy(model);
            mLogger = Logger.Logger.Instance;
        }
        
        public void Estimate(double agentX, double agentY, double agentVx, double agentVy,
            double puckX, double puckY, double puckVx, double puckVy, double PuckR,
            double oppX, double oppY, double oppVx, double oppVy
        )
        {
            AHEntities.SenseEventType newEvent = EstimatorStrategy.Estimate(agentX, agentY, agentVx, agentVy,
            puckX, puckY, puckVx, puckVy, PuckR, oppX, oppY, oppVx, oppVy, WM);

            if (newEvent == SenseEventType.NoEvent)
                return;

            SenseEventArgs senseEvent;
            mLogger.AddLogMessage("Perception: new event detected: " + newEvent.ToString() + 
                                  ", puck velocity: (" + puckVx.ToString() + "," + puckVy.ToString() +")",true);
            if ((newEvent == SenseEventType.yWall) && (puckVx < 0))
            {
                mLogger.AddLogMessage("Perception: Invoking Refining Of Current Plan");
                senseEvent = new SenseEventArgs(SensePlanArg.Refine, newEvent);
            }
            else
            {
                mLogger.AddLogMessage("Perception: Invoking New Planing");
                senseEvent = new SenseEventArgs(SensePlanArg.Plan, newEvent);
            }
            OnEstimationUpdate(this, senseEvent);
        }

        public void Start()
        {
            internalState = ModuleState.Active;
        }

        public void Stop()
        {
            internalState = ModuleState.Inactive;
        }

        public void SubscribeComm(Communicator.Communicator comm)
        {
            comm.IncomingMsg += comm_IncomingMsg;
        }

        private void comm_IncomingMsg(object sender, EventArgs e)
        {
            if (internalState == ModuleState.Active)
            {
                MsgEventArgs msg = e as MsgEventArgs;
                switch (msg.EventCommand)
                {
                    case Command.Message:
                        // agentX, agentY, agentVx, agentVy, puckX, puckY, puckVx, puckVy, puckR, oppX, oppY, oppVx, oppVy
                        string[] vals = msg.EventCommandStr.Split(',');
                        
                        Estimate(float.Parse(vals[0]),
                                 float.Parse(vals[1]),
                                 float.Parse(vals[2]),
                                 float.Parse(vals[3]),
                                 float.Parse(vals[4]),
                                 float.Parse(vals[5]),
                                 float.Parse(vals[6]),
                                 float.Parse(vals[7]),
                                 float.Parse(vals[8]),
                                 float.Parse(vals[9]),
                                 float.Parse(vals[10]),
                                 float.Parse(vals[11]),
                                 float.Parse(vals[12])
                            );
                        break;
                    default:
                        break;
                }
            }
        }


    }
}
