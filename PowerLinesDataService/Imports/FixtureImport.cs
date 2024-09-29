using PowerLinesDataService.Common;
using PowerLinesMessaging;

namespace PowerLinesDataService.Imports;

public class FixtureImport(string source, IFile file, IConnection connection, string queueName) : Import(source, file, connection, queueName)
{
    public override async Task Load(string[] args)
    {
        Console.WriteLine("Importing fixtures");
        await base.Load(args);
    }
}
