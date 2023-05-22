using ChatServer.Net.IO;
using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace ChatServer
{
    public class Client
    {
        public string Username { get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }
        public bool IsOnline;

        PacketReader _packetReader;

        public Client(TcpClient client)
        {
            ClientSocket = client;
            UID = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream());
            
            var opcode = _packetReader.ReadByte();
            Username = _packetReader.ReadMessage();

            Console.WriteLine($"[{DateTime.Now}]: Client has connected with the username: {Username}");
        }

        public void restartClient(TcpClient client)
        {
            ClientSocket = client;
            _packetReader = new PacketReader(ClientSocket.GetStream());

            Console.WriteLine($"[{DateTime.Now}]: Client has connected with the username: {Username}");
            IsOnline = true;
        }

        public void StartListening()
        {
            IsOnline = true;
            Task.Run(() => Process());
        }

        void Process()
        {
            while(true)
            {
                try 
                {
                    string msg;
                    var opcode = _packetReader.ReadByte();
                    switch (opcode)
                    {
                        case 10:
                            _packetReader.ReadMessage(); //receiver is everyone
                            msg = _packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}]: Message from {Username} received! {msg}");
                            //Program.BroadcastMessage($"[{DateTime.Now}]: [{Username}]: {msg}");
                            Program.BroadcastMessage(Username, msg, DateTime.Now.ToString());
                            break;
                        case 11:
                            var receiver = _packetReader.ReadMessage();
                            msg = _packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}]: Message from {Username} for {receiver} received! {msg}");
                            //Program.BroadcastMessage($"[{DateTime.Now}]: [{Username}]: {msg}");
                            Program.SendPersonalMessage(Username, receiver, msg, DateTime.Now.ToString());
                            break;
                        default:
                            break;
                    }
                } 
                catch(Exception e)
                {
                    Console.WriteLine($"[{UID}]: Disconnected!");
                    Program.BroadcastDisconnect(UID.ToString());
                    ClientSocket.Close();
                    ClientSocket.Dispose();               
                    IsOnline = false;
                    break;
                }
            }
        }
    }
}
