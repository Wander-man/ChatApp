using ChatClient.MVVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Windows;

namespace ChatClient.MVVM.ViewModel
{
    class MainViewModel : ObservableObject
    {
        public ObservableCollection<ContactModel> Users { get; set; }
        public ObservableCollection<MessageModel> Messages { get; set; }

        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }

        public string Username { get; set; }
        private string _message;
        public string Message
        {
            get
            { return _message; }
            set
            {
                if (_message == value)
                    return;

                _message = value;
                OnPropertyChanged();
            }
        }

        private ContactModel _selectedContact;

        public ContactModel SelectedContact
        {
            get { return _selectedContact; }
            set { 
                _selectedContact = value;
                OnPropertyChanged();
            }
        }

        private Server _server;

        public MainViewModel()
        {
            Users = new ObservableCollection<ContactModel>
            {
                new ContactModel("All Chat", "0")
            };
            SelectedContact = Users.First();

            Messages = new ObservableCollection<MessageModel>();

            _server = new Server();
            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.userDisconnectedEvent += RemoveUser;
            _server.persMsgReceivedEvent += PersonalMessageReceived;

            ConnectToServerCommand = new RelayCommand(o => _server.ConnectToServer(Username), o => !string.IsNullOrEmpty(Username));
            SendMessageCommand = new RelayCommand(o =>
            {
                _server.SendMessageToServer(Message);
                Message = "";
            }
            , o => !string.IsNullOrEmpty(Message));
        }


        private void RemoveUser()
        {
            var uid = _server.PacketReader.ReadMessage();
            var user = Users.Where(x => x.UID.ToString() == uid).FirstOrDefault();
            Application.Current.Dispatcher.Invoke(() => Users.Remove(user));
        }

        private void MessageReceived()
        {
            // TODO: still accepts string messages
            var sender = _server.PacketReader.ReadMessage();
            var message = _server.PacketReader.ReadMessage();
            var date = _server.PacketReader.ReadMessage();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Users.First().Messages.Add(
                    new MessageModel
                    {
                        Username = sender,
                        Message = message,
                        Time = DateTime.Parse(date)
                    });
                Users.First().UpdateLastMessage();
            });
            

        }

        private void PersonalMessageReceived()
        {
            // doing the sending now to know
            var msg = _server.PacketReader.ReadMessage;
        }

        private void UserConnected()
        {
            //var user = new ContactModel
            //{
            //    Username = _server.PacketReader.ReadMessage(),
            //    UID = _server.PacketReader.ReadMessage(),
            //};

            string username = _server.PacketReader.ReadMessage();
            string uid = _server.PacketReader.ReadMessage();

            if(!Users.Any(x => x.UID == uid))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(new ContactModel(username, uid)));
            }
        }
    }
}
