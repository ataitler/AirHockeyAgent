using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHStrategicLayer
{
    public abstract class IOpponentModelingStrategy
    {
        public abstract void ModelOpponent(StrategicLayer s);
    }
}
