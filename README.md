# PubSub System by Ayu
To run this Publish Subscriber system, you must run 3 individual components:
1. Run PubSub Broker/Broker.cs (Launches Broker server)
2. Run PubSub Publisher/Publisher.cs (Each Publisher.cs instance is a singular publisher)
3. Run PubSub Subscriber/Subscriber.cs (Each Subscriber.cs instance is a singular subscriber)

# Publishers
Publishers will connect to the broker and must type in a topic name. This topic name is binding, and will be what the publisher is publishing to at all times.
After typing a topic, the publisher can freely send messages via the terminal to publish to that topic. Subscribers will see "(topic): (message)" on their end.

# Subscribers
Subscribers do not need to do anything initially except subscribe to desired topics. There is no limit to the number of topics that a subscriber can subscribe to.
See the commands below to learn how to subscribe.

# Commands
Both publishers and subscribers can type /quit to exit.

Subscribers can run commands to subscribe, unsubscribe, list all available topics, and list currently subscribed topics.
The commands are as follows:
1. /subscribe <topic>
2. /unsubscribe <topic>
3. /alltopics
4. /subscribedtopics

Typing an invalid command will display a list of all commands.
