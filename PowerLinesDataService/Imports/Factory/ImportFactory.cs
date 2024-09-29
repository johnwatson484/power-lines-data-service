using PowerLinesDataService.Imports.Readers;
using PowerLinesMessaging;
using PowerLinesDataService.Options;
using Microsoft.Extensions.Options;
using File = PowerLinesDataService.Common.File;

namespace PowerLinesDataService.Imports.Factory;

public class ImportFactory(IOptions<MessageOptions> messageOptions) : IImportFactory
{
    private readonly MessageOptions messageOptions = messageOptions.Value;

    public Import GetImport(ImportType importType, IConnection connection)
    {
        return importType switch
        {
            ImportType.Fixture => new FixtureImport("https://www.football-data.co.uk/fixtures.csv",
                                new File(string.Format("{0}Fixtures_{1}.csv", Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmss")), new FixtureReader()),
                                connection,
                                messageOptions.FixtureQueue
                                ),
            ImportType.Result => new ResultImport("https://www.football-data.co.uk/mmz4281/{0}/{1}.csv",
                                new File(string.Format("{0}Results_{1}.csv", Path.GetTempPath(), DateTime.Now.ToString("yyyyMMddHHmmss")), new ResultReader()),
                                connection,
                                messageOptions.ResultQueue),
            _ => throw new ArgumentException("Import type not found"),
        };
    }
}
