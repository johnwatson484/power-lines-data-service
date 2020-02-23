using System;
using System.Configuration;

namespace PowerLinesDataService.Messaging
{
    public class MessageConfig
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string FixtureQueue { get; set; }

        public string FixtureUsername { get; set; }

        public string FixturePassword { get; set; }

        public string ResultQueue { get; set; }

        public string ResultUsername { get; set; }

        public string ResultPassword { get; set; }
    }
}
