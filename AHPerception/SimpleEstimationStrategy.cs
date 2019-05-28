using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AHEntities;

namespace AHPerception
{
    public class SimpleEstimationStrategy : IEstimationStrategy
    {
        private double eps = 10;
        double zeroVelocityThreshold = 0.01;
        Hashtable global;

        public SimpleEstimationStrategy(WorldModel worldModel)
        {
            global = worldModel.GetConstants();
        }

        public override SenseEventType Estimate(double agentX, double agentY, double agentVx, double agentVy,
                                      double puckX, double puckY, double puckVx, double puckVy, double PuckR,
                                      double oppX, double oppY, double oppVx, double oppVy, WorldModel worldModel)
        {
            Dictionary<string, double> oldState = worldModel.GetPhysicalState();

            SenseEventType event2return = SenseEventType.NoEvent;

            Point puckPold = new Point(oldState["PuckX"], oldState["PuckY"]);
            Point puckPnew = new Point(puckX, puckY);
            Point puckVold = new Point(oldState["PuckVx"], oldState["PuckVy"]);
            Point puckVnew = new Point(puckVx, puckVy);

            Point oppPold = new Point(oldState["OpponentX"], oldState["OpponentY"]);
            Point oppPnew = new Point(oppX, oppY);
            Point agentPold = new Point(oldState["AgentX"], oldState["AgentY"]);
            Point agentPnew = new Point(agentX, agentY);

            worldModel.UpdatePhysicalState(agentX, agentY, agentVx, agentVy, puckX, puckY, puckVx, puckVy, PuckR,
                                               oppX, oppY, oppVx, oppVy);

            if (VelocityChanged(puckVold, puckVnew))
            {
                if ((puckPold.Dist(agentPold) < 80 + eps) || (puckPnew.Dist(agentPnew) < 80 + eps))
                    event2return = SenseEventType.AgentCollision;       // agent gave a hit
                else if ((puckPold.Dist(oppPold) < 80 + eps) || (puckPnew.Dist(oppPnew) < 80 + eps))
                    event2return = SenseEventType.OpponentCollision;    // player gave a hit
                else if (Math.Sign(puckVold.Y) == -Math.Sign(puckVnew.Y))    // Vy_old == -Vy_new
                {
                    if (IsZero(puckVnew.X))                             // puck is stuck on a line (bouncing from side to side)
                    {
                        if (puckPnew.X < -eps)
                            event2return = SenseEventType.StuckAgent;   // stuck on the agent's side
                        else
                            event2return = SenseEventType.StuckPlayer;  // stuck on the player's side
                    }
                    else
                        event2return = SenseEventType.yWall;            // hit one of the side walls
                }
                else if ((IsZero(puckVnew.X)) && (IsZero(puckVnew.Y)))  // stuck (standing still) somewhere
                {
                    if (puckPnew.X < -eps)
                        event2return = SenseEventType.StuckAgent;       // standing still on the agent's side
                    else
                        event2return = SenseEventType.StuckPlayer;      // stading still on the player's side
                }
                else if (Math.Sign(puckVold.X) != Math.Sign(puckVnew.X))
                    event2return = SenseEventType.xWall;                // hit one of the back/front walls
                else
                    event2return = SenseEventType.Disturbance;
            }
            else
                event2return = SenseEventType.NoEvent;
            
            TrajectoryQueue puckTrajectory =  EstimateLineCrossing.EstimatePuckTrajectory(puckPnew, puckVnew, (double)global["PuckRadius"],
                        (int)global["Tablewidth"], (int)global["Tableheight"], (double)global["TimeStep"], (double)global["TimeScale"]);
            worldModel.UpdatePuckTrajectory(puckTrajectory);

            return event2return;
        }

        private bool VelocityChanged(Point PuckVold, Point PuckVnew)
        {
            Point oldVelocity = PuckVold.Normalize();
            Point newVelocity = PuckVnew.Normalize();


            if ((PuckVnew.Norm() <= PuckVold.Norm()) && (PuckVnew.Norm() >= 0.95*PuckVold.Norm()) && (Math.Abs(PuckVnew.Angle() - PuckVold.Angle()) < 0.06))
                return false;
            return true;
        }

        private bool IsZero(double velocityAxis)
        {
            if (Math.Abs(velocityAxis) < zeroVelocityThreshold)
                return true;
            return false;
        }
    }
}
