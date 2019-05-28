using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AHEntities;

namespace AHTacticLayer
{
    public abstract class IActionSelectionStrategy
    {
        public abstract AHEntities.ActionDirective SelectAction(SenseEventType planReason);
    }
}
