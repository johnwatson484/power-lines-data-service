using PowerLinesDataService.Common;
using PowerLinesDataService.Models;
using System.Text.RegularExpressions;

namespace PowerLinesDataService.Imports.Readers;

public class FixtureReader : IReader
{
    public IList<object> ReadToList(string filepath)
    {
        List<Fixture> fixtures = new();

        using (var reader = new StreamReader(filepath))
        {
            var header = reader.ReadLine();
            var headers = header.Split(',');

            int homeAverage = Array.IndexOf(headers, "AvgH");
            int drawAverage = Array.IndexOf(headers, "AvgD");
            int awayAverage = Array.IndexOf(headers, "AvgA");

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                // Source file often contains unicode characters
                line = Regex.Replace(line, @"[^\u0000-\u007F]", string.Empty);
                var values = line.Split(',');

                if (!string.IsNullOrEmpty(values[0].Trim()))
                {
                    fixtures.Add(new Fixture
                    {
                        Division = values[0].Trim(),
                        Date = DateTime.Parse(values[1].Trim()).Add(TimeSpan.Parse(values[2])),
                        HomeTeam = values[3].Trim(),
                        AwayTeam = values[4].Trim(),
                        HomeOddsAverage = homeAverage == -1 ? 0 : !string.IsNullOrEmpty(values[homeAverage].Trim()) ? decimal.Parse(values[homeAverage].Trim()) : 0,
                        DrawOddsAverage = drawAverage == -1 ? 0 : !string.IsNullOrEmpty(values[drawAverage].Trim()) ? decimal.Parse(values[drawAverage].Trim()) : 0,
                        AwayOddsAverage = awayAverage == -1 ? 0 : !string.IsNullOrEmpty(values[awayAverage].Trim()) ? decimal.Parse(values[awayAverage].Trim()) : 0
                    });
                }
            }
        }

        return fixtures.ToList<object>();
    }
}
