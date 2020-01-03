using System;
using PowerLinesDataService.Common;
using PowerLinesDataService.Messaging;

namespace PowerLinesDataService.Imports
{
    public abstract class Import
    {
        protected string source;

        protected IFile file;

        protected IConnection connection;

        protected MessageConfig messageConfig;

        public Import(string source, IFile file, IConnection connection, MessageConfig messageConfig)
        {
            this.source = source;
            this.file = file;
            this.connection = connection;
            this.messageConfig = messageConfig;
        }

        public abstract void Load(string[] args);
    }
}
