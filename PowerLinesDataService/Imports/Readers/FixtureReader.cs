using PowerLinesDataService.Common;
using PowerLinesDataService.Models;

namespace PowerLinesDataService.Imports.Readers;

public class FixtureReader : IReader
{
    public List<string> Diagnostics { get; } = [];

    public IList<object> ReadToList(string filepath)
    {
        Diagnostics.Clear();
        var fixtures = new List<object>();
        using var csv = new SourceCsv(filepath);
        while (csv.Next())
        {
            try
            {
                var fixture = new Fixture { Division = csv.Required("Div"), Date = csv.Date(true), HomeTeam = csv.Required("HomeTeam"), AwayTeam = csv.Required("AwayTeam") };
                if (fixture.HomeTeam == fixture.AwayTeam) throw new FormatException("Identical teams");
                csv.Provenance(fixture, fixture.Division, fixture.Date, fixture.HomeTeam, fixture.AwayTeam);
                var average = csv.Average(fixture);
                fixture.HomeOddsAverage = average?.Home ?? 0; fixture.DrawOddsAverage = average?.Draw ?? 0; fixture.AwayOddsAverage = average?.Away ?? 0;
                fixtures.Add(fixture);
            }
            catch (FormatException ex) { Diagnostics.Add($"Row {csv.Row}: {ex.Message}"); }
        }
        Diagnostics.AddRange(csv.Diagnostics);
        foreach (var diagnostic in Diagnostics) Console.Error.WriteLine(diagnostic);
        return fixtures;
    }
}
