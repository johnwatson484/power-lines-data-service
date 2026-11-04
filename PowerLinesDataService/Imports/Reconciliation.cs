using System.Text.Json;
using PowerLinesDataService.Imports.Readers;
using PowerLinesDataService.Models;

namespace PowerLinesDataService.Imports;

// A stage is a durable, reviewable file. This command has no message-broker or DB connection.
public static class Reconciliation
{
    public static void Stage(string[] args)
    {
        string Get(string key) => args[Array.IndexOf(args, key) + 1];
        var source = Get("--source-file"); var url = Get("--source-url"); var output = Get("--output");
        var reader = new ResultReader(); var rows = reader.ReadToList(source).Cast<Result>().ToList();
        foreach (var row in rows) row.SourceUrl = url;
        var duplicateIdentities = rows.GroupBy(x => x.SourceMatchId).Where(x => x.Count() > 1).Select(x => x.Key).ToHashSet();
        Directory.CreateDirectory(output);
        System.IO.File.WriteAllText(Path.Combine(output, "stage.json"), JsonSerializer.Serialize(new { SourceUrl = url, SourceHash = rows.FirstOrDefault()?.SourceFileHash, Rows = rows.Where(x => !duplicateIdentities.Contains(x.SourceMatchId)), QuarantinedSourceMatchIds = duplicateIdentities.Order(), Diagnostics = reader.Diagnostics, Applied = false }, new JsonSerializerOptions { WriteIndented = true }));
        Console.WriteLine($"Staged {rows.Count} rows; ambiguous identities {duplicateIdentities.Count}; diagnostics {reader.Diagnostics.Count}. No changes applied.");
    }
}
