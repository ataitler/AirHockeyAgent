using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Logger 
{
    public class FileLogger : ILogger
    {
        #region Data
        private string mFileName;
        private StreamWriter mLogFile;

        public string FileName
        {
            get { return mFileName; }
        }
        #endregion

        #region Constructor
        public FileLogger(string fileName)
        {
            mFileName = fileName;
        }
        #endregion

        #region Public methods
        public void Init()
        {
            mLogFile = new StreamWriter(mFileName, true);
        }

        public void Terminate()
        {
            mLogFile.Close();
        }
        #endregion

        #region ILogger Members

        public void ProcessLogMessage(string logMessage)
        {
            // FileLogger implements the ProcessLogMessage method by writing the incoming
            // message to a file.
            mLogFile.WriteLine(logMessage);
        }
        #endregion
    }
}
