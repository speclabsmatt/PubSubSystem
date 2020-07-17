using System;

using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PubSubCommon;

namespace PubSub_Subscriber
{
    class Subscriber
    {
        static async Task Main(string[] args)
        {
            Thread.Sleep(1000);

            try
            {

                Int32 port = 9999;
                TcpClient client = new TcpClient("127.0.0.1", port);

                Console.WriteLine($"subscriber: {client.Client.LocalEndPoint}");

                string message = "CONNECTION-SUBSCRIBER";

                NetworkStream stream = client.GetStream();
                await stream.WriteStringAsync(message);

                _ = ReadFromBroker(client);


                string input = Console.ReadLine();
                while (!input.Equals("/quit"))
                {
                    await stream.WriteStringAsync(input);
                    input = Console.ReadLine();
                }

                stream.Close();
                client.Close();

            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }

        }

        static async Task ReadFromBroker(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            while (client.Connected)
            {
                var message = await stream.ReadStringAsync();
                Console.WriteLine(message);
            }

        }

    }
}
