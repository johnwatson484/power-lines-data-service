using System;
using System.Configuration;

namespace PowerLinesDataService.Messaging
{
    public class MessageConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FixtureQueue { get; set; }
        public string ResultQueue { get; set; }
    }
}
