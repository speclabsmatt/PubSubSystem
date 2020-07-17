using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using PubSubCommon;

namespace PubSub_Broker
{
    class Broker
    {
        private static TcpListener Server;

        private static List<Publisher> PUBLISHERS = new List<Publisher>();
        private static List<Subscriber> SUBSCRIBERS = new List<Subscriber>();

        private const string prefix = "/";

        static async Task Main(string[] args)
        {

            Server = new TcpListener(IPAddress.Any, 9999);

            await ListenForConnectionsAsync();

        }

        public static async Task ListenForConnectionsAsync()
        {
            Server.Start();
            await AcceptConnectionsAsync();
        }

        private static async Task AcceptConnectionsAsync()
        {
            while (true)
            {
                var client = await Server.AcceptTcpClientAsync();
                HandleClient(client);
            }
        }

        private static async void HandleClient(TcpClient client)
        {
            

            var stream = client.GetStream();
            var clientType = await stream.ReadStringAsync();

            Console.WriteLine($"connected: {client.Client.RemoteEndPoint} - {clientType}");

            switch (clientType)
            {
                case "CONNECTION-PUBLISHER":
                    AcceptNewPublisher(client);
                    return;
                case "CONNECTION-SUBSCRIBER":
                    AcceptNewSubscriber(client);
                    return;
            }
        }



        private static async void AcceptNewPublisher(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            
            await stream.WriteStringAsync("Enter the name of the topic that you would like to create and publish to: ");
            
            var topic = await stream.ReadStringAsync();

            await stream.WriteStringAsync("All further messages will now be published to the \"" + topic + "\" topic." +
                "\nType /quit to exit.");

            HandlePublisher(new Publisher(client, topic));
        }

        private static async void HandlePublisher(Publisher publisher)
        {
            PUBLISHERS.Add(publisher);
            TcpClient client = publisher.GetTcpClient();
            NetworkStream stream = client.GetStream();
            while (client.Connected)
            {
                var message = await stream.ReadStringAsync();
                Console.WriteLine("Received publisher message: " + message);
                foreach (var sub in SUBSCRIBERS)
                {
                    if (sub.IsSubscribedToTopic(publisher.GetTopic()))
                        await sub.SendMessageAsync(publisher.GetTopic() + ": " + message);
                }
            }

            PUBLISHERS.Remove(publisher);
        }

        private static async void AcceptNewSubscriber(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            await stream.WriteStringAsync("Type /help for a list of commands.");

            await stream.WriteStringAsync("Type /quit to exit.");

            HandleSubscriber(new Subscriber(client));
        }

        private static async void HandleSubscriber(Subscriber subscriber)
        {
            SUBSCRIBERS.Add(subscriber);
            TcpClient client = subscriber.GetTcpClient();
            NetworkStream stream = client.GetStream();
            while (client.Connected)
            {

                var command = await stream.ReadStringAsync();
                Console.WriteLine("Received subscriber message: " + command);

                string[] cmd = command.Split(" ", 2);

                if (!cmd[0].StartsWith(prefix)) continue;
                cmd[0] = cmd[0].Substring(prefix.Length);
                switch (cmd[0])
                {
                    case "subscribe":
                    case "sub":
                        if (cmd.Length < 2)
                        {
                            await subscriber.SendMessageAsync("That command requires an argument." +
                                "\nType " + prefix + "help for a list of commands.");
                            continue;
                        }
                        if(!GetAvailableTopics().Contains(cmd[1]))
                        {
                            await subscriber.SendMessageAsync("That is not an available topic to subscribe to." +
                                "\nType " + prefix + "help for a list of commands.");
                            continue;
                        }
                        subscriber.SubscribeToTopic(cmd[1]);
                        await subscriber.SendMessageAsync("You have subscribed to topic " + cmd[1]);
                        continue;
                    case "unsubscribe":
                    case "unsub":
                        if (cmd.Length < 2)
                        {
                            await subscriber.SendMessageAsync("That command requires an argument." +
                                "\nType " + prefix + "help for a list of commands.");
                            continue;
                        }
                        if (!subscriber.GetSubscribedTopics().Contains(cmd[1]))
                        {
                            await subscriber.SendMessageAsync("You are not subscribed to that topic." +
                                "\nType " + prefix + "help for a list of commands.");
                            continue;
                        }
                        subscriber.UnsubscribeFromTopic(cmd[1]);
                        await subscriber.SendMessageAsync("You have unsubscribed from topic " + cmd[1]);
                        continue;
                    case "subscribedtopics":
                        await subscriber.SendMessageAsync("Currently subscribed topics: "
                            + GetCommaSeparatedString(subscriber.GetSubscribedTopics()));
                        continue;
                    case "alltopics":
                        await subscriber.SendMessageAsync("Currently available topics: " 
                            + GetCommaSeparatedString(GetAvailableTopics()));
                        continue;
                    case "help":
                        if (cmd.Length == 1)
                        {
                            await subscriber.SendMessageAsync("Available Commands: "
                                + prefix + "subscribe <topic>, "
                                + prefix + "unsubscribe <topic>, "
                                + prefix + "subscribedtopics, and "
                                + prefix + "alltopics");
                        }
                        continue;
                    default:
                        await subscriber.SendMessageAsync("Available Commands: "
                                + prefix + "subscribe <topic>, "
                                + prefix + "unsubscribe <topic>, "
                                + prefix + "subscribedtopics, and "
                                + prefix + "alltopics");
                        continue;
                }
            
            }
            SUBSCRIBERS.Remove(subscriber);
        }

        public static List<string> GetAvailableTopics()
        {
            List<string> topics = new List<string>();
            PUBLISHERS.ForEach(pub => topics.Add(pub.GetTopic()));
            return topics;
        }

        public static string? GetCommaSeparatedString(List<string> list)
        {
            string output = null;

            if(list.Count == 0) return output;
            if(list.Count == 1) return list[0];
            if(list.Count == 2) return list[0] + " and " + list[1];

            output = "";

            int index = 0;
            while(list.Count > 2) {
                output += list[0] + ", ";
                list.RemoveAt(0);
            }

            output += list[0] + " and " + list[1];
            return output;
        }

    }
}