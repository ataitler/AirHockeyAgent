using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHPerception
{
    public interface IPerception
    {
        event EventHandler OnEstimationUpdate;

        void Estimate(double agentX, double agentY, double agentVx, double agentVy,
            double puckX, double puckY, double puckVx, double puckVy, double PuckR,
            double oppX, double oppY, double oppVx, double oppVy
        );

        void Start();

        void Stop();

    }
}
