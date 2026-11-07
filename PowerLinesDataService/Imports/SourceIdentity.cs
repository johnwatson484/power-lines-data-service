using System.Text.RegularExpressions;
using PowerLinesDataService.Imports.Readers;
using PowerLinesDataService.Models;

namespace PowerLinesDataService.Imports;

public static class SourceIdentity
{
    public static void Attach(SourceRecord item, string sourceUrl)
    {
        item.SourceUrl = sourceUrl;
        // The file's season is authoritative even when postponements cross the calendar boundary.
        var season = Regex.Match(sourceUrl, @"/mmz4281/(?<season>\d{4})/");
        if (season.Success) item.SourceSeason = season.Groups["season"].Value;
        var identity = item switch
        {
            Result r => (r.Division, r.HomeTeam, r.AwayTeam),
            Fixture f => (f.Division, f.HomeTeam, f.AwayTeam),
            _ => throw new ArgumentException("Unsupported source record")
        };
        item.SourceMatchId = SourceCsv.Hash($"{item.SourceSeason}|{identity.Division}|{identity.HomeTeam}|{identity.AwayTeam}");
        // Completed results can repeat the same home/away pairing within a season.
        // Their played date distinguishes those matches; fixtures retain rescheduling identity.
        if (item is Result result)
            item.SourceMatchId = SourceCsv.Hash($"{item.SourceSeason}|{identity.Division}|{identity.HomeTeam}|{identity.AwayTeam}|{result.Date:yyyy-MM-dd}");
    }
}
