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
        protected QueueType queueType;
        protected string queue;

        public void CreateConnectionToQueue(QueueType queueType, string brokerUrl, string queue)
        {
            this.queueType = queueType;
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

            channel.BasicPublish(exchange: GetExchangeName(),
                                 routingKey: GetQueueName(),
                                 basicProperties: null,
                                 body: body);
        }

        private void CreateConnectionFactory(string brokerUrl)
        {
            connectionFactory = new ConnectionFactory()
            {
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
            switch (queueType)
            {
                case QueueType.Worker:
                    CreateWorkerQueue();
                    break;
                case QueueType.Exchange:
                    CreateExchange();
                    break;
                default:
                    break;
            }
        }

        private void CreateWorkerQueue()
        {
            channel.QueueDeclare(queue: queue,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);
        }

        private void CreateExchange()
        {
            channel.ExchangeDeclare(queue, ExchangeType.Fanout, true, false);
        }

        private string GetExchangeName()
        {
            return queueType == QueueType.Exchange ? queue : "";
        }

        private string GetQueueName()
        {
            return queueType == QueueType.Worker ? queue : "";
        }
    }
}
