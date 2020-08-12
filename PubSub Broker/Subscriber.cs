using System; // unused
using System.Collections.Generic;
using System.Linq; // unused
using System.Net.Sockets;
using System.Threading.Tasks;
using PubSubCommon;

namespace PubSub_Broker
{
    class Subscriber
    {
        // These two can be made readonly
        private TcpClient tcpClient;
        private List<string> topics = new List<string>();

        // Every function in this class could be converted to an expression body like so, which I think is quite clean. 
        // Not required, but just something to think about.

        public Subscriber(TcpClient tcp) => tcpClient = tcp;

        public TcpClient GetTcpClient() => tcpClient;

        public void SubscribeToTopic(string topic) => topics.Add(topic);

        public void UnsubscribeFromTopic(string topic) => topics.Remove(topic);

        public List<string> GetSubscribedTopics() => topics;

        public bool IsSubscribedToTopic(string topic) => topics.Contains(topic);

        public async Task SendMessageAsync(string message) => 
            await tcpClient.GetStream().WriteStringAsync(message);
    }
    // extra newline
}
