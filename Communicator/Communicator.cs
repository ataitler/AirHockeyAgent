using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Logger;
using AHEntities;
using System.Net;
using System.Net.Sockets;

namespace Communicator
{
    public class Communicator
    {
        private Socket clientSocket;
        private EndPoint epServer;
        private short mID;
        private Logger.Logger mLogger = null;

        byte[] byteData = new byte[1024];

        private static object mLock = new object();

        private static Communicator mCommunicator = null;

        public event EventHandler IncomingMsg;

        public static Communicator Instance
        {
            get
            {
                if (mCommunicator == null)
                {
                    lock (mLock)
                    {
                        if (mCommunicator == null)
                        {
                            mCommunicator = new Communicator();
                        }
                    }
                }
                return mCommunicator;
            }
        }

        private Communicator()
        {
            mLogger = Logger.Logger.Instance;
        }

        public void Init(short ID, IPAddress IP, int Port)
        {
            mID = ID;
            clientSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint ipEndPoint = new IPEndPoint(IP, Port);
            epServer = (EndPoint)ipEndPoint;
        }

        #region IO

        public void Listen()
        {
            byteData = new byte[1024];
            //Start listening to the data asynchronously
            clientSocket.BeginReceiveFrom(byteData,
                                       0, byteData.Length, SocketFlags.None, ref epServer, new AsyncCallback(OnReceive), null);
        }

        public void SendMessage(Command cmd, string msg)
        {
            Data msgToSend = new Data();
            msgToSend.cmdCommand = cmd;
            msgToSend.strMessage = msg;
            msgToSend.sID = this.mID;

            byte[] message = msgToSend.ToByte();
            try
            {
                clientSocket.BeginSendTo(message, 0, message.Length,
                                         SocketFlags.None, epServer, new AsyncCallback(OnSend), null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Data ReceiveMessage()
        {
            return null;
        }

        private void OnSend(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                // write to log file that there was a problem
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndReceive(ar);

                //Convert the bytes received into an object of type Data
                Data msgReceived = new Data(byteData);

                // if a msg from the server, raise incoming msg.
                if (msgReceived.sID == 0)
                {
                    if (IncomingMsg != null)
                    {
                        MsgEventArgs e = new MsgEventArgs(msgReceived.cmdCommand, msgReceived.sID, msgReceived.strMessage);
                        IncomingMsg(this, e);
                    }
                }

                byteData = new byte[1024];
                //Start listening to receive more data from the server
                clientSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epServer,
                                           new AsyncCallback(OnReceive), null);
            }
            catch (Exception ex)
            {
                mLogger.AddLogMessage("Communicator: " + ex.Message);
                mLogger.AddLogMessage(ex.StackTrace.ToString());
            }
        }

        #endregion
    }
}
