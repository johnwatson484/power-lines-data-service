using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.VisualBasic.FileIO;
using PowerLinesDataService.Models;

namespace PowerLinesDataService.Imports.Readers;

// Header indexes and row values always share the source schema, including files without Time.
public sealed class SourceCsv : IDisposable
{
    readonly TextFieldParser parser;
    readonly Dictionary<string, int> headers;
    readonly string hash;
    string[] values = [];
    public long Row { get; private set; } = 1;
    public List<string> Diagnostics { get; } = [];

    public SourceCsv(string path)
    {
        hash = Convert.ToHexString(SHA256.HashData(System.IO.File.ReadAllBytes(path))).ToLowerInvariant();
        parser = new TextFieldParser(path, Encoding.UTF8, true) { HasFieldsEnclosedInQuotes = true, TrimWhiteSpace = true };
        parser.SetDelimiters(",");
        var names = parser.ReadFields() ?? throw new FormatException("Missing CSV header");
        // D2 2004/05 labels three unused Ladbrokes columns identically as "LB".
        // They cannot identify a home/draw/away price and are not consumed by this reader.
        headers = names.Select((x, i) => (Name: x.Trim().TrimStart('\uFEFF'), Index: i))
            .Where(x => x.Name.Length > 0 && !x.Name.Equals("LB", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => x.Name, x => x.Index, StringComparer.OrdinalIgnoreCase);
        // Older Greek files use HT/AT for the home and away team names.
        foreach (var (name, legacy) in new[] { ("HomeTeam", "HT"), ("AwayTeam", "AT") })
            if (!headers.ContainsKey(name) && headers.TryGetValue(legacy, out var index)) headers[name] = index;
        foreach (var required in new[] { "Div", "Date", "HomeTeam", "AwayTeam" })
            if (!headers.ContainsKey(required)) throw new FormatException($"Missing header {required}");
    }

    public bool Next()
    {
        while (!parser.EndOfData)
        {
            Row++;
            try
            {
                values = parser.ReadFields() ?? [];
                if (values.All(string.IsNullOrWhiteSpace)) continue;
                return true;
            }
            catch (MalformedLineException ex) { Diagnostics.Add($"Row {Row}: malformed CSV: {ex.Message}"); }
        }
        return false;
    }

    public string Get(string name) => headers.TryGetValue(name, out var i) && i < values.Length ? values[i].Trim() : "";
    public string Required(string name) => Get(name) is { Length: > 0 } value ? value : throw new FormatException($"Missing {name}");
    public int? Integer(string name) => Get(name).Length == 0 ? null : int.TryParse(Get(name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) && n >= 0 ? n : throw new FormatException($"Invalid {name}");
    public decimal Price(string name) => decimal.TryParse(Get(name), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var p) && p > 1 ? p : 0;

    public DateTime Date(bool kickoff)
    {
        if (!DateTime.TryParseExact(Required("Date"), ["dd/MM/yyyy", "d/M/yyyy", "dd/MM/yy", "d/M/yy", "yyyy-MM-dd"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            throw new FormatException("Invalid Date");
        if (kickoff && Get("Time").Length > 0)
        {
            if (!TimeSpan.TryParseExact(Get("Time"), [@"h\:mm", @"hh\:mm"], CultureInfo.InvariantCulture, out var time) || time.TotalDays >= 1)
                throw new FormatException("Invalid Time");
            date = date.Add(time);
        }
        return date;
    }

    public void Provenance(SourceRecord item, string division, DateTime date, string home, string away)
    {
        var year = date.Month >= 7 ? date.Year : date.Year - 1;
        item.SourceSeason = $"{year % 100:00}{(year + 1) % 100:00}";
        item.SourceFileHash = hash;
        item.SourceRowId = Hash($"{division}|{date:yyyy-MM-dd}|{home}|{away}");
        // Stable for rescheduling/renaming only when source identity or an explicit alias resolves it.
        item.SourceMatchId = Hash($"{item.SourceSeason}|{division}|{home}|{away}");
        foreach (var (prefix, kind) in new[] { ("Avg", "AveragePreClosing"), ("BbAv", "AveragePreClosing"), ("AvgC", "AverageClosing"), ("Max", "MaximumPreClosing"), ("BbMx", "MaximumPreClosing"), ("MaxC", "MaximumClosing"), ("B365", "BookmakerPreClosing"), ("B365C", "BookmakerClosing"), ("PS", "BookmakerPreClosing"), ("PSC", "BookmakerClosing") })
        {
            var h = Price(prefix + "H"); var d = Price(prefix + "D"); var a = Price(prefix + "A");
            if (h > 1 && d > 1 && a > 1) item.SourcePrices.Add(new SourcePrice(prefix, kind, h, d, a));
        }
        item.PriceKind = item.SourcePrices.Any(x => x.Columns is "Avg" or "BbAv") ? "AveragePreClosing" : "Unknown";
        item.PriceCollection = "Football-Data source convention; individual quote observation time unknown";
    }

    public SourcePrice Average(SourceRecord item) => item.SourcePrices.FirstOrDefault(x => x.Columns is "Avg" or "BbAv");
    public static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    public void Dispose() => parser.Dispose();
}
