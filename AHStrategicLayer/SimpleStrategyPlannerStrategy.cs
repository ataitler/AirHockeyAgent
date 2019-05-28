using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AHEntities;

namespace AHStrategicLayer
{
    public class SimpleStrategyPlannerStrategy : IStrategyPlannerStrategy
    {
        private WorldModel worldModel;

        public SimpleStrategyPlannerStrategy(WorldModel Wm)
        {
            worldModel = Wm;
        }

        public override Dictionary<string, double> PlanStrategy()
        {
            Dictionary<string, double> strategy = new Dictionary<string, double>()
            {
                {"GameStyle", 0},
                {"PLeft", (double)1/3},
                {"PMiddle", (double)1/3},
                {"PRight", (double)1/3},
            };
            return strategy;
        }
    }
}
