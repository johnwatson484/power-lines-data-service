using PowerLinesDataService.Common;
using PowerLinesDataService.Models;
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

        var failedSources = new List<string>();
        var unavailableSources = new List<string>();
        int importedSources = 0;
        using var httpClient = new HttpClient();
        while (firstSeason < lastSeason)
        {
            foreach (var league in leagues)
            {
                var url = string.Format(source, GetSeasonYears(firstSeason % 100), league);
                try
                {
                    try
                    {
                        using var request = new HttpRequestMessage(HttpMethod.Get, url);
                        using var response = await GetResponse(httpClient, request);
                        using Stream contentStream = await response.Content.ReadAsStreamAsync(),
                        stream = new FileStream(file.Filepath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
                        await contentStream.CopyToAsync(stream);
                    }
                    catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
                    {
                        Console.WriteLine($"Unavailable source {url}: {ex.Message}");
                        unavailableSources.Add(url);
                        continue;
                    }
                    Console.WriteLine($"Reading source {url}");
                    SendSource(file.ReadFileToList(), url);
                    importedSources++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error importing {url}: {ex}");
                    failedSources.Add(url);
                }
                finally
                {
                    file.DeleteFileIfExists();
                }
            }

            firstSeason++;
        }
        Console.WriteLine($"Results import summary: imported={importedSources}, unavailable={unavailableSources.Count}, failed={failedSources.Count}");
        if (failedSources.Count > 0) throw new InvalidOperationException("Failed source imports: " + string.Join(", ", failedSources));
        Console.WriteLine("Import complete");
    }

    private static string GetSeasonYears(int firstYear)
    {
        return string.Format("{0}{1}", firstYear.ToString("D2"), ((firstYear + 1) % 100).ToString("D2"));
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
