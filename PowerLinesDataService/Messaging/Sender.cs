using System;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;

namespace PowerLinesDataService.Messaging
{
    public class Sender : ISender
    {
        protected ConnectionFactory connectionFactory;
        protected RabbitMQ.Client.IConnection connection;
        protected IModel channel;
        protected string queue;

        public void CreateConnectionToQueue(string brokerUrl, string queue)
        {
            this.queue = queue;
            CreateConnectionFactory(brokerUrl);
            CreateConnection();
            CreateChannel();
            CreateQueue();
        }

        public void CloseConnection()
        {
            Console.WriteLine("CLOSE CONNECTION");
            connection.Close();
        }

        public void SendMessage(object obj)
        {
            var message = JsonConvert.SerializeObject(obj);
            Console.WriteLine("SEND MESSAGE");         
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "",
                                 routingKey: queue,
                                 basicProperties: null,
                                 body: body);
        }

        private void CreateConnectionFactory(string brokerUrl)
        {
            connectionFactory = new ConnectionFactory() {
                Uri = new Uri(brokerUrl)
            };
        }

        private void CreateConnection()
        {
            Console.WriteLine("CREATE CONNECTION");
            connection = connectionFactory.CreateConnection();
        }

        private void CreateChannel()
        {
            channel = connection.CreateModel();
        }

        private void CreateQueue()
        {
            Console.WriteLine("CREATE QUEUE");
            channel.QueueDeclare(queue: queue,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }
    }
}
