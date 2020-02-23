namespace PowerLinesDataService.Messaging
{
    public interface ISender
    {
        void CreateConnectionToQueue(string hostname, string queue);

        void CloseConnection();

        void SendMessage(object obj);
    }
}
