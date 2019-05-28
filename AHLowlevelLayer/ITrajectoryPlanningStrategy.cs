using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AHEntities;

namespace AHLowlevelLayer
{
    public abstract class ITrajectoryPlanningStrategy
    {
        public abstract double[][,] TrajectoryPlanning(PointParams initialConditions, PointParams finalConditions);
    }
}
