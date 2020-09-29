using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using ENet;
using TestEnetClient.Models;

namespace TestEnetClient
{
    public class Program
    {
        private const string ServerHostName = "localhost";
        private const int ServerPort = 8888;
        private const int SendTicksThreshold = 5000000;

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

                var isConnected = false;
                var lastSentTicks = DateTime.Now.Ticks;
                var random = new Random();

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
                                isConnected = true;
                                break;

                            case EventType.Disconnect:
                                Console.WriteLine("Client disconnected from server");
                                isConnected = false;
                                break;

                            case EventType.Timeout:
                                Console.WriteLine("Client connection timeout");
                                isConnected = false;
                                break;

                            case EventType.Receive:
                                Console.WriteLine("Packet received from server - Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);
                                netEvent.Packet.Dispose();
                                break;
                        }
                    }
                
                    if (isConnected && DateTime.Now.Ticks - lastSentTicks> SendTicksThreshold)
                    {
                        var data = new Player
                        {
                            HeadX = random.Next(),
                            HeadY = random.Next(),
                            HeadZ = random.Next()
                        };

                        var packet = new Packet();
                        packet.Create(PlayerToByte(data));

                        peer.Send(0, ref packet);
                        lastSentTicks = DateTime.Now.Ticks;

                        Console.WriteLine("Packet sent at " + lastSentTicks + " ticks");

                        packet.Dispose();
                    }

                    client.Flush();
                }
            }
        }

        private static byte[] PlayerToByte(Player player)
        {
            var json = JsonSerializer.Serialize(player);
            var encodedPlayer = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            var bytes = Convert.FromBase64String(encodedPlayer);

            return bytes;
        }
    }
}
