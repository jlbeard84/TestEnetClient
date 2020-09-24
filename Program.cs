using System;
using ENet;

namespace TestEnetClient
{
    public class Program
    {
        private const string ServerHostName = "localhost";
        private const int ServerPort = 8888;

        public static void Main(string[] args)
        {
            using (var client = new Host()) 
            {
                Console.WriteLine("Starting server...");

                var address = new Address
                {
                    Port = ServerPort
                };

                address.SetHost(ServerHostName);

                client.Create();

                Console.WriteLine($"Connecting to host {ServerHostName} on {address.Port}");

                var peer = client.Connect(address);

                Event netEvent;

                while (!Console.KeyAvailable) 
                {
                    var polled = false;

                    while (!polled) 
                    {
                        if (client.CheckEvents(out netEvent) <= 0) 
                        {
                            if (client.Service(15, out netEvent) <= 0)
                            {
                                break;
                            }

                            polled = true;
                        }

                        switch (netEvent.Type) 
                        {
                            case EventType.None:
                                break;

                            case EventType.Connect:
                                Console.WriteLine("Client connected to server");
                                break;

                            case EventType.Disconnect:
                                Console.WriteLine("Client disconnected from server");
                                break;

                            case EventType.Timeout:
                                Console.WriteLine("Client connection timeout");
                                break;

                            case EventType.Receive:
                                Console.WriteLine("Packet received from server - Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);
                                netEvent.Packet.Dispose();
                                break;
                        }
                    }
                }

                client.Flush();
            }
        }
    }
}
