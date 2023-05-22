﻿using ChatClient.Net.IO;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ChatClient.Net
{
    class Server
    {
        TcpClient _client;
        public PacketReader PacketReader;

        public event Action connectedEvent;
        public event Action msgReceivedEvent;
        public event Action userDisconnectedEvent;
        public event Action persMsgReceivedEvent;

        public Server()
        {
            _client= new TcpClient();
        }

        public void ConnectToServer(string username)
        {
            if (!_client.Connected)
            {
                _client.Connect("127.0.0.1", 7891);
                PacketReader = new PacketReader(_client.GetStream());

                if(!string.IsNullOrEmpty(username))
                {
                    var connectPacket = new PacketBuilder();
                    connectPacket.WriteOpCode(0);
                    connectPacket.WriteMessage(username);
                    _client.Client.Send(connectPacket.GetPacketBytes());
                }

                ReadPackets();



            }
        }

        private void ReadPackets()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    var opcode = PacketReader.ReadByte();
                    switch (opcode)
                    {
                        case 1:
                            connectedEvent?.Invoke();
                            break;

                        case 5:
                            userDisconnectedEvent?.Invoke();
                            break;

                        case 10:
                            msgReceivedEvent?.Invoke();
                            break;

                        case 11: 
                            persMsgReceivedEvent?.Invoke();
                            break;

                        default:
                            Console.WriteLine("Default, idk what to do");
                            break;

                    }
                }
            });
        }

        public void SendMessageToServer(string message)
        {
            var messagePacket = new PacketBuilder();
            messagePacket.WriteOpCode(10);
            messagePacket.WriteMessage(message);
            _client.Client.Send(messagePacket.GetPacketBytes());
        }
    }
}
