using System;
using System.Net;
using System.Net.Sockets;

namespace ChatServer
{
    class Program
    {
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
            }

            // TODO: Broadcast the connection;
        }
    }
}


