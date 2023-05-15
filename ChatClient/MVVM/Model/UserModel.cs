

using System;
using System.Linq;

namespace ChatClient.MVVM.Model
{
    // To be removed
    class UserModel
    {
        public string Username { get; set; }
        public string UID { get; set; }
    }

    // A contact class to be used
    class ContactModel
    {
        public string Username { get; set; }       
        public string UID { get; set; }
        public System.Collections.ObjectModel.ObservableCollection<MessageModel> Messages { get; set; }
        public string LastMessage => Messages.Last().Message;
    }
}
