using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHEntities
{
    public class MsgEventArgs : EventArgs
    {
        private Command m_EventCommand;
        private short m_EventSenderID;
        private string m_EventCommandStr;

        public Command EventCommand
        {
            get { return m_EventCommand; }
            set { m_EventCommand = value; }
        }
        public short EventSenderID
        {
            get { return m_EventSenderID; }
            set { m_EventSenderID = value; }
        }
        public string EventCommandStr
        {
            get { return m_EventCommandStr; }
            set { m_EventCommandStr = value; }
        }

        public MsgEventArgs() { }

        public MsgEventArgs(Command cmd, short ID, string msg)
        {
            EventCommand = cmd;
            EventCommandStr = msg;
            EventSenderID = ID;
        }
    }
}
