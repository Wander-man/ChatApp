

using System;
using System.Linq;

namespace ChatClient.MVVM.Model
{

    // A contact class to be used
    class ContactModel
    {
        public string Username { get; set; }       
        public string UID { get; set; }
        public System.Collections.ObjectModel.ObservableCollection<MessageModel> Messages { get; set; } = new System.Collections.ObjectModel.ObservableCollection<MessageModel> ();
        public string LastMessage => Messages.Last().Message;
    }
}
