namespace PowerLinesDataService.Messaging
{
    public interface ISender
    {
        void CreateConnectionToQueue(QueueType queueType, string brokerUrl, string queue);

        void CloseConnection();

        void SendMessage(object obj);
    }
}
