using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AHEntities;

namespace AHLowlevelLayer
{
    public abstract class IActionPlanningStrategy
    {
        public abstract PointParams ActionPlanning(AHEntities.ActionDirective A, bool isNewPlaning);
    }
}
