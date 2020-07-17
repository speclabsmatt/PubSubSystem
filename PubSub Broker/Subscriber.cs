using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using PubSubCommon;

namespace PubSub_Broker
{
    class Subscriber
    {

        private TcpClient tcpClient;
        private List<string> topics = new List<string>();

        public Subscriber(TcpClient tcp)
        {

            tcpClient = tcp;

        }

        public TcpClient GetTcpClient()
        {

            return tcpClient;

        }

        public void SubscribeToTopic(string topic)
        {

            topics.Add(topic);

        }

        public void UnsubscribeFromTopic(string topic)
        {

            topics.Remove(topic);

        }

        public List<string> GetSubscribedTopics()
        {

            return topics;

        }

        public bool IsSubscribedToTopic(string topic)
        {

            return topics.Contains(topic);

        }

        public async Task SendMessageAsync(string message)
        {
            await tcpClient.GetStream().WriteStringAsync(message);
        }

    }

}
