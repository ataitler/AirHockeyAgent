using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using AHEntities;

namespace AHStrategicLayer
{
    public class StrategicLayer
    {
        private Logger.Logger mLogger;
        AHEntities.WorldModel WM;
        ModuleState internalState;
        IOpponentModelingStrategy OpponentModelingStrategy;
        IStrategyPlannerStrategy StrategyPlannerStrategy;
        Thread OpponentModelingThread;
        Dictionary<string, double> strategy;

        public StrategicLayer(WorldModel wolrdModel)
        {
            WM = wolrdModel;
            strategy = new Dictionary<string, double>(WM.getStrategy());
            mLogger = Logger.Logger.Instance;
            internalState = ModuleState.Inactive;
            OpponentModelingStrategy = new SimpleOpponentModelingStrategy();
            //StrategyPlannerStrategy = new SimpleStrategyPlannerStrategy(WM);
            StrategyPlannerStrategy = new NeuralStrategyPlannerStrategy(WM);
            this.Init();
        }

        public void Init()
        {
            // initialize all strategic and statistical parameters.
        }
        
        public ModuleState Start()
        {
            if (internalState == ModuleState.Inactive)
            {
                OpponentModelingThread = new Thread(() => OpponentModelingStrategy.ModelOpponent(this));
                OpponentModelingThread.Start();
                internalState = ModuleState.Active;
            }
            return internalState;
        }

        public ModuleState Stop()
        {
            if (internalState == ModuleState.Active)
                OpponentModelingThread.Abort();
            internalState = ModuleState.Inactive;
            return internalState;
        }

        public void UpdateGameStrategy()
        {
            // plan "new" strategy
            Dictionary<string, double> s = StrategyPlannerStrategy.PlanStrategy();
            
            // check if old strategy has changed
            if (StrategyEquals(s, strategy) == false)
            {
                strategy = s;
                mLogger.AddLogMessage("Strategics: New strategy planned: " + PrintStrategy(s));

                // update World Model
                WM.UpdateStrategy(s["GameStyle"], s["PLeft"], s["PMiddle"], s["PRight"]);
            }
        }

        private string PrintStrategy(Dictionary<string, double> s)
        {
            string str = string.Empty;
            foreach (KeyValuePair<string, double> entry in s)
            {
                str = str + entry.Key.ToString() + ":" + entry.Value.ToString() + ", ";
            }
            return str;
        }

        private bool StrategyEquals(Dictionary<string, double> s1, Dictionary<string, double> s2)
        {
            if (s1.Count == s2.Count) // Require equal count.
            {
                foreach (var pair in s1)
                {
                    double value;
                    if (s2.TryGetValue(pair.Key, out value))
                    {
                        // Require value be equal.
                        if (value != pair.Value)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        // Require key be present.
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

    }
}
