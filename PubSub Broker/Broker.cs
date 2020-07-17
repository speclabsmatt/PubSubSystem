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
using System.Net.Http;

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

            Console.WriteLine($"Connected: {client.Client.RemoteEndPoint} - {clientType}");

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

            while(GetAvailableTopics().Contains(topic))
            {
                await stream.WriteStringAsync("That topic already exists. Please choose a different topic: ");

                topic = await stream.ReadStringAsync();
            }

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
                if (message.Length < 1) continue;
                Console.WriteLine($"Publisher {client.Client.RemoteEndPoint}: " + message);

                if(message.Equals(prefix + "quit"))
                {

                    stream.Close();
                    client.Close();
                    continue;

                }

                foreach (var sub in SUBSCRIBERS)
                {
                    if (sub.IsSubscribedToTopic(publisher.GetTopic()))
                        await sub.SendMessageAsync(publisher.GetTopic() + ": " + message);
                }
            }

            PUBLISHERS.Remove(publisher);

            SUBSCRIBERS.ForEach(subscriber =>
            {
                if (subscriber.IsSubscribedToTopic(publisher.GetTopic()))
                    subscriber.UnsubscribeFromTopic(publisher.GetTopic());
            });
        }

        private static async void AcceptNewSubscriber(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            await stream.WriteStringAsync("Type " + prefix + "help for a list of commands." +
                "\nType " + prefix + "quit to exit.");

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
                if (command.Length < 1) continue;
                Console.WriteLine($"Subscriber {client.Client.RemoteEndPoint}: " + command);

                string[] cmd = command.Split(" ", 2);

                if (!cmd[0].StartsWith(prefix))
                {
                    await subscriber.SendMessageAsync("Type " + prefix + "help for a list of commands.");
                    continue;
                }
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
                                "\nType " + prefix + "alltopics to see a list of available topics to subscribe to.");
                            continue;
                        }
                        if(subscriber.IsSubscribedToTopic(cmd[1]))
                        {
                            await subscriber.SendMessageAsync("You are already subscribed to that topic.");
                            continue;
                        }
                        subscriber.SubscribeToTopic(cmd[1]);
                        await subscriber.SendMessageAsync("You have subscribed to topic \"" + cmd[1] + "\"");
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
                                "\nType " + prefix + "subscribedtopics to see a list of your currently subscribed topics.");
                            continue;
                        }
                        subscriber.UnsubscribeFromTopic(cmd[1]);
                        await subscriber.SendMessageAsync("You have unsubscribed from topic \"" + cmd[1] + "\"");
                        continue;

                    case "subscribedtopics":
                        await subscriber.SendMessageAsync("Currently subscribed topics: "
                            + GetCommaSeparatedString(subscriber.GetSubscribedTopics()));
                        continue;

                    case "alltopics":
                    case "topics":
                        await subscriber.SendMessageAsync("Currently available topics: " 
                            + GetCommaSeparatedString(GetAvailableTopics()));
                        continue;

                    case "help":
                        if (cmd.Length == 1)
                        {
                            await subscriber.SendMessageAsync("Available Commands: "
                                + prefix + "subscribe <topic>, "
                                + prefix + "unsubscribe <topic>, "
                                + prefix + "subscribedtopics, "
                                + prefix + "alltopics, and "
                                + prefix + "quit");
                        }
                        continue;

                    case "quit":
                        stream.Close();
                        client.Close();
                        continue;

                    default:
                        await subscriber.SendMessageAsync("That is not a valid command. " +
                            "Type " + prefix + "help for a list of commands.");
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