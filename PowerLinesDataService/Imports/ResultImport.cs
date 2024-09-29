using PowerLinesDataService.Common;
using PowerLinesMessaging;

namespace PowerLinesDataService.Imports;

public class ResultImport(string source, IFile file, IConnection connection, string queueName) : Import(source, file, connection, queueName)
{
    public override async Task Load(string[] args)
    {
        Console.WriteLine("Importing results");

        bool currentSeasonOnly = !args.Contains("--all");

        int firstSeason = GetFirstSeasonYear(DateTime.Now, currentSeasonOnly);
        int lastSeason = GetLastSeasonYear(DateTime.Now);

        while (firstSeason < lastSeason)
        {
            foreach (var league in leagues)
            {
                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        using var request = new HttpRequestMessage(HttpMethod.Get, string.Format(source, GetSeasonYears(firstSeason % 100), league));
                        using Stream contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync(),
                        stream = new FileStream(file.Filepath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
                        await contentStream.CopyToAsync(stream);
                    }
                    SendToQueue(file.ReadFileToList());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error importing: {0}", ex);
                }
                finally
                {
                    file.DeleteFileIfExists();
                }
            }

            firstSeason++;
        }
        Console.WriteLine("Import complete");
    }

    private static string GetSeasonYears(int firstYear)
    {
        return string.Format("{0}{1}", firstYear.ToString("D2"), (firstYear + 1).ToString("D2"));
    }

    private static int GetFirstSeasonYear(DateTime currentDate, bool currentSeasonOnly = false)
    {
        if (!currentSeasonOnly)
        {
            return 1993;
        }

        // handle covid 19 season changes
        if (currentDate.Month <= 5 || (currentDate.Year == 2020 && currentDate.Month <= 8))
        {
            return currentDate.Year - 1;
        }
        return currentDate.Year;
    }

    private static int GetLastSeasonYear(DateTime currentDate)
    {
        if (currentDate.Month <= 5)
        {
            return currentDate.Year;
        }
        return currentDate.Year + 1;
    }

    private readonly List<string> leagues = new()
    {
        "E0",
        "E1",
        "E2",
        "E3",
        "EC",
        "SC0",
        "SC1",
        "SC2",
        "SC3",
        "D1",
        "D2",
        "I1",
        "I2",
        "SP1",
        "SP2",
        "F1",
        "F2",
        "N1",
        "B1",
        "P1",
        "T1",
        "G1"
    };
}
