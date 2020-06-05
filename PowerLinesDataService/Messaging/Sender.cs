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
            connection.Close();
        }

        public void SendMessage(object obj)
        {
            var message = JsonConvert.SerializeObject(obj);            
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
            connection = connectionFactory.CreateConnection();
        }

        private void CreateChannel()
        {
            channel = connection.CreateModel();
        }

        private void CreateQueue()
        {
            channel.QueueDeclare(queue: queue,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }
    }
}
