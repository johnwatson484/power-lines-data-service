using System;
using System.Threading.Tasks;
using PowerLinesDataService.Common;
using PowerLinesDataService.Messaging;
using PowerLinesMessaging;

namespace PowerLinesDataService.Imports
{
    public class FixtureImport : Import
    {
        public FixtureImport(string source, IFile file, IConnection connection, string queueName) : base(source, file, connection, queueName)
        {
        }

        public override void Load(string[] args)
        {
            Console.WriteLine("Importing fixtures");
            base.Load(args);
        }
    }
}
