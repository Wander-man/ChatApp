using ChatServer.Net.IO;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    class Program
    {
        // TODO:
        // Saving of message history
        // Private messages
        // Default buttons for the window
        // Two way binding for message in textbox done
            

        static List<Client> _users;
        static TcpListener _listener;
        private static void Main(string[] args)
        {
            _users = new List<Client>();
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 7891);
            _listener.Start();

            while(true)
            {
                Client client = new Client(_listener.AcceptTcpClient());
                _users.Add(client);
                BroadcastConnection();
            }

                        
        }

        // TODO: this nested loop thing is kinda unnecesarry.
        // (If we are going keep stuff on clients anyway)
        // maybe for a first launch or sth but not every time someone connects;
        static void BroadcastConnection()
        {
            foreach (var user in _users)
            {
                foreach (var usr in _users)
                {
                    var broadcastPacket = new PacketBuilder();
                    broadcastPacket.WriteOpCode(1);
                    broadcastPacket.WriteMessage(usr.Username);
                    broadcastPacket.WriteMessage(usr.UID.ToString());
                    user.ClientSocket.Client.Send(broadcastPacket.GetPacketBytes());
                }
            }
        }

        // The broadcast messages are not persistant for new users to see
        // So that's to be fixed
        public static void BroadcastMessage(string message)
        {
            var msgPacket = new PacketBuilder();
            msgPacket.WriteOpCode(10);
            msgPacket.WriteMessage(message);

            foreach (var user in _users)
            {
                user.ClientSocket.Client.Send(msgPacket.GetPacketBytes());
            }
        }
        
        // Now that I look at this I should put the "broadcasting of sth" in a smaller func;
        // Or just add the opcode parameter to the BroadcastMessage;
        public static void BroadcastDisconnect(string uid)
        {
            var disconnectedUser = _users.Where(x => x.UID.ToString() == uid).FirstOrDefault();
            _users.Remove(disconnectedUser);

            var disPacket = new PacketBuilder();
            disPacket.WriteOpCode(5);
            disPacket.WriteMessage(uid);

            foreach (var user in _users)
            {
                user.ClientSocket.Client.Send(disPacket.GetPacketBytes());
            }

            BroadcastMessage($"[{disconnectedUser.Username}] has disconnected!");
        }

    }
}


