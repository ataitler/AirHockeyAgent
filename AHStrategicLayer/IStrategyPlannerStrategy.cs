using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHStrategicLayer
{
    public abstract class IStrategyPlannerStrategy
    {
        public abstract Dictionary<string, double> PlanStrategy();
    }
}
