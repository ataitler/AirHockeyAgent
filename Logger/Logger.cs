using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public class Logger
    {
        #region Data
        private static object mLock = new object();

        private static Logger mLogger = null;

        public static Logger Instance
        {
            get
            {
                // If this is the first time we’re referring to the
                // singleton object, the private variable will be null.
                if (mLogger == null)
                {
                    // for thread safety, lock an object when
                    // instantiating the new Logger object. This prevents
                    // other threads from performing the same block at the
                    // same time.
                    lock (mLock)
                    {
                        // Two or more threads might have found a null
                        // mLogger and are therefore trying to create a 
                        // new one. One thread will get to lock first, and
                        // the other one will wait until mLock is released.
                        // Once the second thread can get through, mLogger
                        // will have already been instantiated by the first
                        // thread so test the variable again. 
                        if (mLogger == null)
                        {
                            mLogger = new Logger();
                        }
                    }
                }
                return mLogger;
            }
        }

        private List<ILogger> mObservers;

        #endregion

        #region Constructor
        private Logger()
        {
            mObservers = new List<ILogger>();
        }
        #endregion

        #region Public methods
        public void RegisterObserver(ILogger observer)
        {
            if (!mObservers.Contains(observer))
            {
                mObservers.Add(observer);
            }
        }

        public void AddLogMessage(string message,bool newLine=false)
        {
            // Apply some basic formatting like the current timestamp
            //string formattedMessage = string.Format("{0} - {1}", DateTime.Now.ToString(), message);
            string newline = string.Empty;
            if (newLine)
                newline = "\r\n";
            string formattedMessage = newline + string.Format("{0} - {1}",
                                      DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"), message);
            foreach (ILogger observer in mObservers)
            {
                observer.ProcessLogMessage(formattedMessage);
            }
        }

        public void AddLogMessage(Exception ex)
        {
            StringBuilder message = new StringBuilder(ex.Message);
            string trace = ex.StackTrace;

            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
                message.Append("\n" + ex.Message);
            }
            message.Append("\nStack trace: " + trace);

            AddLogMessage(message.ToString());
        }
        #endregion
    }
}
