using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AHEntities;

namespace AHStrategicLayer
{
    public class NeuralStrategyPlannerStrategy : IStrategyPlannerStrategy
    {
        private WorldModel worldModel;
        private double[] w_attack;
        private double[] w_defend;

        public NeuralStrategyPlannerStrategy(WorldModel WM)
        {
            worldModel = WM;
            w_attack = new double[] {-0.6618, -0.8261, -2.0176, 3.0873 };
            w_defend = new double[] { -0.7541, -3.3007, 7.7786, -3.6127 };
        }

        public override Dictionary<string, double> PlanStrategy()
        {
            List<double> currentGameState = worldModel.GetGameState();
            
            // augemnt bias to the space
            currentGameState.Insert(0, 1);
            double style = 0.0;
            double class1 = 0.0;
            double class2 = 0.0;
            for (int i = 0; i < currentGameState.Count; i++)
            {
                class1 = class1 + currentGameState[i] * w_attack[i];
                class2 = class2 + currentGameState[i] * w_defend[i];
            }
            if (Math.Sign(class1) == 1)
                style = 3.0;
            else if (Math.Sign(class2) == 1)
                style = 2.0;
            else
                style = 1.0;

            Dictionary<string, double> strategy = new Dictionary<string, double>()
            {
                {"GameStyle", style},
                {"PLeft", (double)1/3},
                {"PMiddle", (double)1/3},
                {"PRight", (double)1/3},
            };
            return strategy;
            
        }
    }
}
