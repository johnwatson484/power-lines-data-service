using System;
using System.Threading.Tasks;
using Amqp;
using Amqp.Framing;
using Newtonsoft.Json;

namespace PowerLinesDataService.Messaging
{
    public class AmqpConnection : IConnection
    {
        protected ConnectionFactory factory;
        protected Address brokerAddr;
        protected Connection connection;
        protected Session session;
        protected SenderLink sender;

        public async Task CreateConnectionToQueue(string brokerUrl, string queue)
        {
            if (factory == null)
            {
                ConfigureConnectionFactory();
            }
            brokerAddr = new Address(brokerUrl);
            connection = await factory.CreateAsync(brokerAddr);
            session = new Session(connection);
            sender = new SenderLink(session, "sender", queue);
        }

        private void ConfigureConnectionFactory()
        {
            factory = new ConnectionFactory();
            factory.AMQP.ContainerId = "data-service-container";
        }

        public void CloseConnection()
        {
            sender.Close();
            session.Close();
            connection.Close();
        }

        public void SendMessage(object obj)
        {
            var body = JsonConvert.SerializeObject(obj);
            Console.WriteLine("Sending message: {0}", body);
            sender.Send(new Message(body));
            Console.WriteLine("Message sent");
        }
    }
}
