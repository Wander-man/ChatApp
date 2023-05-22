using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    [Serializable]
    class MessageModel
    {
        public string From;
        public string To;
        public string Message;
        public string Time;
        public MessageModel(string from, string to, string message, string time)
        { 
            From = from;
            To = to;                
            Message = message;
            Time = time;
        }
    }
}
