using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHEntities
{
    public class Data
    {
        //Default constructor
        public Data()
        {
            this.cmdCommand = Command.Null;
            this.strMessage = null;
            this.sID = 9;
            //this.strName = null;
        }

        //Converts the bytes into an object of type Data
        public Data(byte[] data)
        {
            //The first four bytes are for the Command
            this.cmdCommand = (Command)BitConverter.ToInt32(data, 0);

            // the next 2 bytes are the players ID
            this.sID = BitConverter.ToInt16(data, 4);

            //The next four store the length of the message
            int msgLen = BitConverter.ToInt32(data, 6);

            //This checks for a null message field
            if (msgLen > 0)
                this.strMessage =
                    Encoding.UTF8.GetString(data, 10, msgLen);
            else
                this.strMessage = null;
        }

        //Converts the Data structure into an array of bytes
        public byte[] ToByte()
        {
            List<byte> result = new List<byte>();

            //First four are for the Command
            result.AddRange(BitConverter.GetBytes((int)cmdCommand));

            // Add 2 bytes for the players ID
            result.AddRange(BitConverter.GetBytes((Int16)sID));

            //Length of the message
            if (strMessage != null)
                result.AddRange(
                    BitConverter.GetBytes(strMessage.Length));
            else
                result.AddRange(BitConverter.GetBytes(0));

            //And, lastly we add the message 
            //text to our array of bytes
            if (strMessage != null)
                result.AddRange(Encoding.UTF8.GetBytes(strMessage));

            return result.ToArray();
        }

        //ID of the player in the game, the left player is always player one
        public Int16 sID;
        //Message text
        public string strMessage;
        //Command type (login, logout, send message, etc)
        public Command cmdCommand;
    }
}
