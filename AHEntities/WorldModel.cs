using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHEntities
{
    public class WorldModel
    {
        Dictionary<string, double> physicalState;
        Dictionary<string, int> global;
        Dictionary<string, double> strategy;
        Dictionary<string, int> delays;
        Hashtable constants;
        TrajectoryQueue puckEstimatedTrajectory;

        private Object physicalStateLock;
        private Object globalLock;
        private Object strategyLock;
        private Object constantsLock;
        private Object puckTrajectoryLock;
        private Object delaysLock;

        public WorldModel()
        {
            #region Locks Init
            physicalStateLock = new Object();
            globalLock = new Object();
            strategyLock = new Object();
            constantsLock = new Object();
            puckTrajectoryLock = new Object();
            delaysLock = new Object();
            #endregion Locks Init

            #region puck trajectory
            // puck always have only 2 degrees of freedom (x,y) since its only an endeffector.
            puckEstimatedTrajectory = new TrajectoryQueue(2, new double[2]{0,0});
            #endregion puck trajectory

            #region physical state
            physicalState = new Dictionary<string, double>()
            {
                {"AgentX", -1000.0},
                {"AgentY", 0.0},
                {"AgentVx", 0.0},
                {"AgentVy", 0.0},
                {"PuckX", 0.0},
                {"PuckY", 0.0},
                {"PuckVx", 0.0},
                {"PuckVy", 0.0},
                {"PuckR", 0.0},
                {"OpponentX", 1000.0},
                {"OpponentY", 0.0},
                {"OpponentVx", 0.0},
                {"OpponentVy", 0.0}
            };
            #endregion physical state

            #region global data
            global = new Dictionary<string, int>()
            {
                {"AgentScore", 0},
                {"OpponentScore", 0},
            };
            #endregion global data

            #region strategy
            strategy = new Dictionary<string, double>()
            {
                {"GameStyle", 0},
                {"PLeft", (double)1/3},
                {"PMiddle", (double)1/3},
                {"PRight", (double)1/3},
            };
            #endregion strategy

            #region game constants
            //constants = new Dictionary<string,int>()
            constants = new Hashtable()
            {
                {"MaxScore", 20},
                {"Tablewidth", 2000},
                {"Tableheight", 1000},
                {"FrameRate", 50},
                {"TimeStep", 0.01},    
                {"TimeScale",1},
                {"PuckRadius", 32},
                {"MalletRadius", 47.5},
                {"PlanPeriod", 0.2},
                {"MoveInterval", 0.8}
            };
            #endregion game constants

            #region Delays
            delays = new Dictionary<string, int>()
            {
                {"Measurement", 0},
                {"Control", 0}
            };
            #endregion Delays
        }

        public void UpdatePhysicalState(double agentX, double agentY, double agentVx, double agentVy,
                                        double puckX, double puckY, double puckVx, double puckVy, double PuckR,
                                        double oppX, double oppY, double oppVx, double oppVy)
        {
            lock (physicalStateLock)
            {
                physicalState["AgentX"] = agentX;
                physicalState["AgentY"] = agentY;
                physicalState["AgentVx"] = agentVx;
                physicalState["AgentVy"] = agentVy;
                physicalState["PuckX"] = puckX;
                physicalState["PuckY"] = puckY;
                physicalState["PuckVx"] = puckVx;
                physicalState["PuckVy"] = puckVy;
                physicalState["PuckR"] = PuckR;
                physicalState["OpponentX"] = oppX;
                physicalState["OpponentY"] = oppY;
                physicalState["OpponentVx"] = oppVx;
                physicalState["OpponentVy"] = oppVy;
            }
        }

        public void UpdateStrategy(double style, double pLeft, double pMiddle, double pRight)
        {
            double sum = pLeft + pMiddle + pRight;
            lock (strategyLock)
            {
                strategy["PLeft"] = pLeft / sum;
                strategy["PMiddle"] = pMiddle / sum;
                strategy["PRight"] = pRight / sum;
                strategy["GameStyle"] = style;
            }
        }

        public void UpdateScore(int agentScore, int oppScore)
        {
            lock (globalLock)
            {
                global["AgentScore"] = agentScore;
                global["OpponentScore"] = oppScore;
            }
        }

        public void UpdatePuckTrajectory(TrajectoryQueue puckT)
        {
            lock (puckTrajectoryLock)
            {
                puckEstimatedTrajectory.Clear();
                puckEstimatedTrajectory.Replace(puckT);
            }
        }

        public void SetConstants(int maxScore, int TableW, int TableH, int frameRate, double timeStep ,double timeScale, double puckR,
                                 double malletR, int delayMeasurement, int delayControl, double actionPlanPeriod, double maxMoveInterval)
        {
            lock (constantsLock)
            {
                constants["MaxScore"] = maxScore;
                constants["Tablewidth"] = TableW;
                constants["Tableheight"] = TableH;
                constants["FrameRate"] = frameRate;
                constants["TimeStep"] = timeStep;
                constants["TimeScale"] = timeScale;
                constants["PuckRadius"] = puckR;
                constants["PlanPeriod"] = actionPlanPeriod;
                constants["MoveInterval"] = maxMoveInterval;
            }
            lock (delaysLock)
            {
                delays["Measurement"] = delayMeasurement;
                delays["Control"] = delayControl;
            }
        }

        public void SetMaxScore(int maxScore)
        {
            lock (constantsLock)
            {
                constants["MaxScore"] = maxScore;
            }
        }

        public Dictionary<string, double> GetPhysicalState()
        {
            Dictionary<string, double> phyState;
            lock (physicalStateLock)
            {
                phyState = new Dictionary<string, double>(physicalState);
            }
            return phyState;
        }

        public int[] GetSize()
        {
            int[] size;
            lock (constantsLock)
            {
                size = new int[2] {(int)constants["Tablewidth"], (int)constants["Tableheight"]};
            }
            return size;
        }

        public Hashtable GetConstants()
        {
            //Dictionary<string,int> consts;
            Hashtable consts;
            lock (constantsLock)
            {
                consts = new Hashtable(constants);
            }
            return consts;
        }

        public List<double> GetGameState()
        {
            List<double> state = new List<double>();

            lock (constantsLock)
            {
                state.Add(Convert.ToDouble(constants["MaxScore"]));
            }
            lock (globalLock)
            {
                state.Add(global["AgentScore"]);
                state.Add(global["OpponentScore"]);
            }

            return state;
        }

        public Dictionary<string, double> getStrategy()
        {
            Dictionary<string, double> strat;

            lock (strategyLock)
            {
                strat = new Dictionary<string, double>(strategy);
            }
            return strat;
        }

        public TrajectoryQueue GetPuckTrajectory()
        {
            // to be updated
            return puckEstimatedTrajectory;
        }
    }
}
