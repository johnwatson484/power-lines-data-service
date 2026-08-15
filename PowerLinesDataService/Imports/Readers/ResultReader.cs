using System.Text.RegularExpressions;
using PowerLinesDataService.Common;
using PowerLinesDataService.Models;

namespace PowerLinesDataService.Imports.Readers;

public class ResultReader : IReader
{
    public IList<object> ReadToList(string filepath)
    {
        List<Result> results = new();

        using (var reader = new StreamReader(filepath))
        {
            var header = reader.ReadLine();
            var headers = header.Split(',');

            int time = Array.IndexOf(headers, "Time");

            // The source dropped the Betbrain columns for AvgH/AvgD/AvgA part way through the 2018/19 season.
            int homeAverage = FindColumn(headers, "AvgH", "BbAvH");
            int drawAverage = FindColumn(headers, "AvgD", "BbAvD");
            int awayAverage = FindColumn(headers, "AvgA", "BbAvA");

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();

                // Older seasons pad the file with trailing blank rows, which have no columns to insert into.
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // Source file often contains unicode characters
                line = Regex.Replace(line, @"[^\u0000-\u007F]", string.Empty);
                var values = line.Split(',');

                if (time == -1)
                {
                    values = InsertIntoArray(values, 2, string.Empty);
                }

                if (values.Length > 6 && !string.IsNullOrEmpty(values[7]))
                {
                    results.Add(new Result
                    {
                        Division = values[0].Trim(),
                        Date = DateTime.Parse(values[1].Trim()),
                        HomeTeam = values[3].Trim(),
                        AwayTeam = values[4].Trim(),
                        FullTimeHomeGoals = int.Parse(values[5].Trim()),
                        FullTimeAwayGoals = int.Parse(values[6].Trim()),
                        FullTimeResult = values[7].Trim(),
                        HalfTimeHomeGoals = values.Length > 8 && !string.IsNullOrEmpty(values[8].Trim()) ? int.Parse(values[8].Trim()) : 0,
                        HalfTimeAwayGoals = values.Length > 8 && !string.IsNullOrEmpty(values[9].Trim()) ? int.Parse(values[9].Trim()) : 0,
                        HalfTimeResult = values.Length > 8 && !string.IsNullOrEmpty(values[10].Trim()) ? values[10].Trim() : null,
                        HomeOddsAverage = homeAverage == -1 ? 0 : !string.IsNullOrEmpty(values[homeAverage].Trim()) ? decimal.Parse(values[homeAverage].Trim()) : 0,
                        DrawOddsAverage = drawAverage == -1 ? 0 : !string.IsNullOrEmpty(values[drawAverage].Trim()) ? decimal.Parse(values[drawAverage].Trim()) : 0,
                        AwayOddsAverage = awayAverage == -1 ? 0 : !string.IsNullOrEmpty(values[awayAverage].Trim()) ? decimal.Parse(values[awayAverage].Trim()) : 0
                    });
                }
            }
        }

        return results.ToList<object>();
    }

    private static int FindColumn(string[] headers, params string[] names)
    {
        foreach (var name in names)
        {
            int index = Array.IndexOf(headers, name);

            if (index != -1)
            {
                return index;
            }
        }

        return -1;
    }

    private static string[] InsertIntoArray(string[] array, int position, string value)
    {
        var values = array.ToList();

        // A malformed row can have fewer columns than the insert position, e.g. a stray short line in the source file.
        values.Insert(Math.Min(position, values.Count), value);
        return values.ToArray();
    }
}
