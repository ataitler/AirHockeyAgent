using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AHEntities;

namespace AHLowlevelLayer
{
    public abstract class IMotionGuidanceStrategy
    {
        public abstract void MotionGuidance();

        public abstract ModuleState Activate();

        public abstract ModuleState Deactivate();
    }
}
