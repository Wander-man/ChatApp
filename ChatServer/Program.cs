using ChatServer.Net.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace ChatServer
{
    class Program
    {
        // TODO:
        // Saving of message history
        // Private messages
        // Default buttons for the window
        // Two way binding for message in textbox done

        static List<MessageModel> _messageHistory;
        static List<Client> _users;
        static TcpListener _listener;
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(SaveHistory);
            _messageHistory = new List<MessageModel>();
            LoadHistory();
            

            _users = new List<Client>();
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
            _listener.Start();

            while(true)
            {
                var tcpClient = _listener.AcceptTcpClient();
                Client client = new Client(tcpClient);
                if(_users.Any(x => x.Username == client.Username))
                {
                    var user = _users.First(x => x.Username == client.Username);
                    if (!user.IsOnline)
                    {
                        user.restartClient(tcpClient);
                        user.StartListening();
                        BroadcastConnection(client.Username);
                        UpdateUserOnHistory(client.Username);
                    }
                    continue;
                } 
                else
                {
                    _users.Add(client);
                    client.StartListening();
                    BroadcastConnection(client.Username);
                    UpdateUserOnHistory(client.Username);
                }
                
            }

                        
        }

        static void AddToHistory(MessageModel message)
        {
            _messageHistory.Add(message);
        }

        static void SortHistory()
        {
            _messageHistory.Sort((a,b) => DateTime.Parse(a.Time).CompareTo(DateTime.Parse(b.Time)));
        }

        static void SaveHistory(object? sender, EventArgs? e)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("../../../Logs/MessageHistoryLog.txt", FileMode.OpenOrCreate, FileAccess.Write);

            foreach (var message in _messageHistory)
            {
                formatter.Serialize(stream, message);
            }
            stream.Close();
        }

        static void LoadHistory()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream("../../../Logs/MessageHistoryLog.txt", FileMode.OpenOrCreate, FileAccess.Read);
            
            while(true)
            {
                try
                {
                    MessageModel message = (MessageModel)formatter.Deserialize(stream);
                    AddToHistory(message);
                } catch (SerializationException e)
                {
                    break;
                }
            }

            stream.Close();
        }

        static void UpdateUserOnHistory(string username)
        {
            SortHistory();
            foreach (MessageModel m in _messageHistory)
            {
                if (m.To == "All Chat" && m.From != "Server")
                {
                    BroadcastMessage(m.From, m.Message, m.Time, username);
                    continue;
                }
                else
                {
                    if (m.From != "Server")
                    {
                        SendPersonalMessage(m.From, m.To, m.Message, m.Time, username);
                        continue;
                    }
                }
            }
            return;
        }

        static void BroadcastConnection(string username)
        {
            var connectee = _users.First(x => x.Username == username);
            foreach (var user in _users)
            {
                foreach(var usr in _users)
                {
                    if (usr == user) continue;
                    
                    var broadcastPacket = new PacketBuilder();
                    broadcastPacket.WriteOpCode(1);
                    broadcastPacket.WriteMessage(usr.Username);
                    broadcastPacket.WriteMessage(usr.UID.ToString());
                    if(user.IsOnline)
                        user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                }
            }
            BroadcastMessage("Server", $"[{connectee.Username}] has connected!", DateTime.Now.ToString());
        }
    

        // The broadcast messages are not persistant for new users to see
        // So that's to be fixed
        //public static void BroadcastMessage(string message)
        //{
        //    var msgPacket = new PacketBuilder();
        //    msgPacket.WriteOpCode(10);
        //    msgPacket.WriteMessage(message);

        //    foreach (var user in _users)
        //    {
        //        user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
        //    }
        //}

        public static void BroadcastMessage(string username, string message, string date, string sendTo = null)
        {
            var msgPacket = new PacketBuilder();
            msgPacket.WriteOpCode(10);
            msgPacket.WriteMessage(username);
            msgPacket.WriteMessage(message);
            msgPacket.WriteMessage(date);

            if (sendTo != null)
            {
                _users.First(x => x.Username == sendTo).ClientSocket.Client.Send(msgPacket.GetPacketBytes());
                return;
            }
            else
            {
                foreach (var user in _users)
                {
                    if (user.IsOnline)
                        user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
                }
            }


            AddToHistory(new MessageModel(username, "All Chat", message, date));
        }

        internal static void SendPersonalMessage(string sender, string receiver, string message, string date, string sendTo = null)
        {
            // letting the client know their message has been sent and displaying it
            
            var fromPacket = new PacketBuilder();
            var toPacket = new PacketBuilder();

            fromPacket.WriteOpCode(12);
            fromPacket.WriteMessage(receiver);
            fromPacket.WriteMessage(message);
            fromPacket.WriteMessage(date);
            if (sendTo == null)
            {
                var user = _users.First(x => x.Username == sender);
                if (user.IsOnline)
                    user.ClientSocket.Client.Send(fromPacket.GetPacketBytes());
            }



            if (sender != receiver)
            {
                // sending the message to the receiver
                toPacket.WriteOpCode(11);
                toPacket.WriteMessage(sender);
                toPacket.WriteMessage(message);
                toPacket.WriteMessage(date);
                if (sendTo == null)
                {
                    var user = _users.First(x => x.Username == receiver);
                    if (user.IsOnline)
                        user.ClientSocket.Client.Send(toPacket.GetPacketBytes());
                }
            }

            if (sendTo != null)
            {
                var client = _users.First(x => x.Username == sendTo).ClientSocket.Client;
                if(sendTo == sender) client.Send(fromPacket.GetPacketBytes());
                if(sendTo == receiver) client.Send(toPacket.GetPacketBytes());

                return;
            }

            AddToHistory(new MessageModel(sender, receiver, message, date));
        }

                
        // Now that I look at this I should put the "broadcasting of sth" in a smaller func;
        // Or just add the opcode parameter to the BroadcastMessage;
        public static void BroadcastDisconnect(string uid)
        {
            var disconnectedUser = _users.Where(x => x.UID.ToString() == uid).FirstOrDefault();
            disconnectedUser.IsOnline = false;
            //_users.Remove(disconnectedUser);

            var disPacket = new PacketBuilder();
            disPacket.WriteOpCode(5);
            disPacket.WriteMessage(uid);

            foreach (var user in _users)
            {
                if (user.IsOnline)
                    user.ClientSocket.Client.Send(disPacket.GetPacketBytes());
            }

            BroadcastMessage("Server", $"[{disconnectedUser.Username}] has disconnected!", DateTime.Now.ToString());
        }


    }
}


