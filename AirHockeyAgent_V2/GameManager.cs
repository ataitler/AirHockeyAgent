using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AHEntities;
using AHLowlevelLayer;
using AHStrategicLayer;
using AHTacticLayer;
using AHPerception;
using Logger;
using Communicator;
using System.Net;
using System.Net.Sockets;

namespace AirHockeyAgent
{
    class GameManager
    {
        #region Member Params
        string mLogFileName;
        string mIP;
        short mID;
        int mPort;
        GameState mState;
        double timeStep;
        double timeScale;
        short fps;
        short DOF;
        int maxScore;
        double puckRadius;
        double malletRadius;
        double actionPlanPeriod;
        double maxMoveInterval;
        int width, height;
        int delayMeasurement, delayControl;
        double planPeriod;
        #endregion Member Params

        #region Member Modules
        WorldModel WM;
        Perception mVisualSensing;
        AHStrategicLayer.StrategicLayer mStrategyLayer;
        AHTacticLayer.TacticLayer mTacticsLayer;
        AHLowlevelLayer.LowLevelLayer mLowLevelLayer;
        #endregion Member Modules

        // logger
        private Logger.Logger mLogger = null;
        // a logger observer that will write the log entries to a file
        private Logger.FileLogger mFileLogger = null;
        // communicator
        private Communicator.Communicator mComm = null;

        public GameManager()
        {
            mState = GameState.Disconnected;

            timeScale = 1;              // absulote
            timeStep = 0.01;            // seconds
            fps = 100;                  // frames pre second (1/fps seconds between frames)
            DOF = 2;                    // degrees of freedom (2 - end-effector X-Y)
            maxScore = 2;               // score to win
            puckRadius = 32;            // mm
            malletRadius = 47.5;        // mm
            width = 2360;               // mm
            height = 1180;              // mm
            delayMeasurement = 0;       // ms
            delayControl = 0;           // ms
            actionPlanPeriod = 0.1;     // seconds
            maxMoveInterval = 0.8;      // seconds
        }

        public void Init()
        {
            mComm = Communicator.Communicator.Instance;
            mComm.IncomingMsg += AHMessageManager;

            mState = GameState.Disconnected;
            WM = new WorldModel();
            WM.SetConstants(maxScore, width, height, fps, timeStep, timeScale, puckRadius, malletRadius,delayMeasurement, delayControl,
                            actionPlanPeriod, maxMoveInterval);
            
            mVisualSensing = new Perception(WM);
            mVisualSensing.SubscribeComm(mComm);

            mLowLevelLayer = new LowLevelLayer(mComm, WM, timeStep, DOF);

            mTacticsLayer = new TacticLayer(WM, mLowLevelLayer);
            mVisualSensing.OnEstimationUpdate += mTacticsLayer.StartNewPlan;

            mStrategyLayer = new StrategicLayer(WM);
        }

        public void Clean()
        {
            if (mLogger != null)
            {
                mFileLogger.Terminate();
            }
        }

        public bool Connect(int port, string IP, short ID, string Logfile)
        {
            mLogFileName = Logfile;
            mID = ID;
            mIP = IP;
            mPort = port;
            
            #region logger
            try
            {
                if (mLogger == null)
                {
                    // instantiate the logger
                    mLogger = Logger.Logger.Instance;

                    // instantiate the log observer that will write to disk
                    mFileLogger = new FileLogger(this.mLogFileName);
                    mFileLogger.Init();

                    // Add mFileLogger to the Logger.
                    mLogger.RegisterObserver(mFileLogger);
                    mLogger.AddLogMessage("******* NEW RUN OF THE AGENT! *******");
                }
            }
            catch (Exception)
            {
                Clean();
                return false;
            }
            #endregion logger

            #region Communication
            try
            {
                IPAddress ipAddress = IPAddress.Parse(mIP);
                mComm.Init(mID, ipAddress, mPort);
                mComm.SendMessage(Command.Login, mID.ToString());
                mComm.Listen();
            }
            catch (Exception ex)
            {
                mLogger.AddLogMessage("Communication Init: "+ ex.Message);
                if (mLogger != null)
                {
                    mFileLogger.Terminate();
                }
                return false;
            }
            #endregion Communication

            // connection established successfully
            AHStateManager(GameState.Idle);

            return true;
        }

        public void Disconnect()
        {
            AHStateManager(GameState.Disconnected);
            // clear data
        }

        public void AHMessageManager(object sender, EventArgs e)
        {
            MsgEventArgs msgE = e as MsgEventArgs;
            if (msgE.EventCommand == Command.Message)
                return;
            switch (msgE.EventCommand)
            {
                case Command.Start:
                    // check if should update something
                    mLogger.AddLogMessage("GameManager Msg: Start Msg Received with: " + msgE.EventCommandStr);
                    this.AHStateManager(GameState.Playing);
                    break;

                case Command.Reset:
                    mLogger.AddLogMessage("GameManager Msg: Reset Msg Received with: " + msgE.EventCommandStr);
                    break;

                case Command.Pause:
                    mLogger.AddLogMessage("GameManager Msg: Pause Msg Received with: " + msgE.EventCommandStr);
                    break;

                case Command.Score:
                    // update episodes parameters
                    mLogger.AddLogMessage("GameManager Msg: Score Msg Received with: " + msgE.EventCommandStr);
                    string[] vals = msgE.EventCommandStr.Split(':');
                    WM.UpdateScore(Convert.ToInt32(vals[0]), Convert.ToInt32(vals[1]));
                    AHStateManager(GameState.GameIdle);
                    break;

                case Command.Login:
                    //initialize agent
                    mLogger.AddLogMessage("GameManager Msg: Login Msg Received with: " + msgE.EventCommandStr);
                    AHStateManager(GameState.Idle);
                    break;

                case Command.NewGame:
                    // initialize game
                    mLogger.AddLogMessage("GameManager Msg: NewGame Msg Received with: " + msgE.EventCommandStr);
                    WM.SetMaxScore(Convert.ToInt32(msgE.EventCommandStr));
                    WM.UpdateScore(0, 0);
                    AHStateManager(GameState.GameIdle);
                    break;

                case Command.EndGame:
                    // handle end of game - clear/save data
                    mLogger.AddLogMessage("GameManager Msg: EndGame Msg Received with: " + msgE.EventCommandStr);
                    AHStateManager(GameState.Idle);
                    break;

                default:
                    mLogger.AddLogMessage("GameManagerMsg: defauld Msg Received - " + msgE.EventCommand.ToString());
                    break;
            }
        }

        public void AHStateManager(GameState s)
        {
            switch (s)
            {
                case GameState.Disconnected:
                    // clear?
                    mLowLevelLayer.Stop();
                    mVisualSensing.Stop();
                    mTacticsLayer.Stop();
                    mStrategyLayer.Stop();
                    mState = GameState.Disconnected;
                    mLogger.AddLogMessage("GameManager State: Game state is Disconnected");
                    mComm.SendMessage(Command.Logout, null);
                    break;

                case GameState.GameIdle:
                    mLowLevelLayer.Stop();
                    mVisualSensing.Stop();
                    mTacticsLayer.Stop();
                    mStrategyLayer.Stop();
                    this.mState = GameState.GameIdle;
                    mLogger.AddLogMessage("GameManager State: Game state is GameIdle");
                    break;

                case GameState.Idle:
                    mLowLevelLayer.Stop();
                    mVisualSensing.Stop();
                    mTacticsLayer.Stop();
                    mStrategyLayer.Stop();
                    mStrategyLayer.Init();
                    mState = GameState.Idle;
                    mLogger.AddLogMessage("GameManager State: Game state is Idle");
                    break;

                case GameState.Playing:
                    mLowLevelLayer.Start();
                    mStrategyLayer.Start();
                    mTacticsLayer.Start();
                    mVisualSensing.Start();
                    //World Model re init
                    mState = GameState.Playing;
                    mLogger.AddLogMessage("GameManager State: Game state is Playing");
                    break;
            }
        }
    }

}
