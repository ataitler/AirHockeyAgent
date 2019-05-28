using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AHEntities;

namespace AHPerception
{
    public abstract class IEstimationStrategy
    {
        public abstract SenseEventType Estimate(double agentX, double agentY, double agentVx, double agentVy,
            double puckX, double puckY, double puckVx, double puckVy, double PuckR,
            double oppX, double oppY, double oppVx, double oppVy, WorldModel worldModel);
    }
}
