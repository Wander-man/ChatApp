using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace ChatClient.MVVM.Model
{
    class MessageModel
    {
        public string Username { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
        //public bool IsNativeOrigin { get; set; }          
        //public bool? FirstMessage { get; set; }
        public string MessagePrint {
            get
            {
                return toString();
            }
        }

        public string toString()
        {
            return ($"[{Time}]: [{Username}]: {Message}");
        }
    }
}
