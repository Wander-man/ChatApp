

using ChatClient.MVVM.Core;
using System;
using System.ComponentModel;
using System.Linq;

namespace ChatClient.MVVM.Model
{
    // A contact class to be used
    class ContactModel : ObservableObject
    {
        public string Username { get; set; }
        public string UID { get; set; }
        private string lastMessage;
        public string LastMessage { 
            get
            {
                return lastMessage;
            }
            set
            {
                lastMessage = value;
                OnPropertyChanged();
            }
        }

        private System.Collections.ObjectModel.ObservableCollection<MessageModel> messages;
        public System.Collections.ObjectModel.ObservableCollection<MessageModel> Messages
        { 
            get
            {
                return messages;
            }
            set 
            {
                messages = value;
                OnPropertyChanged();
            }
        }

        public void UpdateLastMessage()
        {
            var last = Messages.Last();
            LastMessage = last.Username + ": " + last.Message;
        }

        public ContactModel (string username, string uid)
        {
            Messages = new System.Collections.ObjectModel.ObservableCollection<MessageModel>();
            Username = username;
            UID = uid;
        }    
    }
}
