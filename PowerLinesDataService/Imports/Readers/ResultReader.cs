using PowerLinesDataService.Common;
using PowerLinesDataService.Models;

namespace PowerLinesDataService.Imports.Readers;

public class ResultReader : IReader
{
    public List<string> Diagnostics { get; } = [];

    public IList<object> ReadToList(string filepath)
    {
        Diagnostics.Clear();
        var results = new List<object>();
        using var csv = new SourceCsv(filepath);
        while (csv.Next())
        {
            try
            {
                var homeGoals = csv.Integer("FTHG") ?? csv.Integer("HG") ?? throw new FormatException("Missing full-time home goals");
                var awayGoals = csv.Integer("FTAG") ?? csv.Integer("AG") ?? throw new FormatException("Missing full-time away goals");
                var result = new Result
                {
                    Division = csv.Required("Div"), Date = csv.Date(false),
                    HomeTeam = csv.Required("HomeTeam"), AwayTeam = csv.Required("AwayTeam"),
                    FullTimeHomeGoals = homeGoals, FullTimeAwayGoals = awayGoals,
                    FullTimeResult = csv.Get("FTR").Length > 0 ? csv.Get("FTR") : csv.Required("Res"),
                    HalfTimeHomeGoals = csv.Integer("HTHG"), HalfTimeAwayGoals = csv.Integer("HTAG"),
                    HalfTimeResult = csv.Get("HTR").Length > 0 ? csv.Get("HTR") : null
                };
                if (result.HomeTeam == result.AwayTeam || result.FullTimeResult != (homeGoals > awayGoals ? "H" : homeGoals < awayGoals ? "A" : "D"))
                    throw new FormatException("Inconsistent result/teams");
                csv.Provenance(result, result.Division, result.Date, result.HomeTeam, result.AwayTeam);
                var average = csv.Average(result);
                result.HomeOddsAverage = average?.Home ?? 0;
                result.DrawOddsAverage = average?.Draw ?? 0;
                result.AwayOddsAverage = average?.Away ?? 0;
                results.Add(result);
            }
            catch (FormatException ex) { Diagnostics.Add($"Row {csv.Row}: {ex.Message}"); }
        }
        Diagnostics.AddRange(csv.Diagnostics);
        foreach (var diagnostic in Diagnostics) Console.Error.WriteLine(diagnostic);
        return results;
    }
}
