using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace AHStrategicLayer
{
    class SimpleOpponentModelingStrategy : IOpponentModelingStrategy
    {
        public override void ModelOpponent(StrategicLayer s)
        {
            // load updated parameters from s
            while (true)
            {
                Thread.Sleep(1000);
                s.UpdateGameStrategy();
            }
        }
    }
}
