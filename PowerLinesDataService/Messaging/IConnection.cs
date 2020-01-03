using System;
using System.Threading.Tasks;

namespace PowerLinesDataService.Messaging
{
    public interface IConnection
    {
        Task CreateConnectionToQueue(string brokerUrl, string queue);

        void CloseConnection();

        void SendMessage(object obj);
    }
}
