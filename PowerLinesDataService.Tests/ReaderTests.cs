using NUnit.Framework;
using PowerLinesDataService.Imports.Readers;
using PowerLinesDataService.Imports;
using PowerLinesDataService.Models;

namespace PowerLinesDataService.Tests;

public class ReaderTests
{
    static (ResultReader Reader, List<Result> Rows) Parse(string csv)
    {
        var file = Path.GetTempFileName();
        try { System.IO.File.WriteAllText(file, csv); var reader = new ResultReader(); return (reader, reader.ReadToList(file).Cast<Result>().ToList()); }
        finally { System.IO.File.Delete(file); }
    }

    [Test]
    public void MissingTimeDoesNotShiftAverageColumns()
    {
        var (_, rows) = Parse("Div,Date,HomeTeam,AwayTeam,FTHG,FTAG,FTR,BbMxH,BbAvH,BbMxD,BbAvD,BbMxA,BbAvA\nE0,10/08/18,Man United,Leicester,2,1,H,1.60,1.56,4.20,3.92,8.05,7.06\n");
        Assert.That(rows.Single().HomeOddsAverage, Is.EqualTo(1.56m));
        Assert.That(rows.Single().DrawOddsAverage, Is.EqualTo(3.92m));
        Assert.That(rows.Single().AwayOddsAverage, Is.EqualTo(7.06m));
        Assert.That(rows.Single().SourcePrices.Select(x => x.Kind), Is.EquivalentTo(new[] { "AveragePreClosing", "MaximumPreClosing" }));
        Assert.That(rows.Single().HalfTimeHomeGoals, Is.Null);
    }

    [Test]
    public void QuotedCommasUnicodeBomAndClosingArePreserved()
    {
        var (_, rows) = Parse("\uFEFFDiv,Date,Time,HomeTeam,AwayTeam,FTHG,FTAG,FTR,AvgH,AvgD,AvgA,AvgCH,AvgCD,AvgCA\nE0,01/02/2024,15:00,\"München, FC\",Équipe,0,0,D,2.1,3.2,4.3,2.2,3.3,4.4\n\n");
        Assert.That(rows.Single().HomeTeam, Is.EqualTo("München, FC"));
        Assert.That(rows.Single().HomeOddsAverage, Is.EqualTo(2.1m));
        Assert.That(rows.Single().SourcePrices.Count, Is.EqualTo(2));
        Assert.That(rows.Single().SourceRowId, Has.Length.EqualTo(64));
    }

    [Test]
    public void TruncatedAndInvalidRowsHaveDiagnosticsAndNeverInventPrices()
    {
        var (reader, rows) = Parse("Div,Date,HomeTeam,AwayTeam,FTHG,FTAG,FTR,AvgH,AvgD,AvgA\nE0,31/02/2024,A,B,1,0,H,2,3,4\nE0\nE0,01/02/2024,A,B,1,0,H,2\n");
        Assert.That(reader.Diagnostics.Count, Is.EqualTo(2));
        Assert.That(rows.Single().HomeOddsAverage, Is.Zero);
    }
    [Test]
    public void Complete_E0_2018_19_source_selects_380_averages_instead_of_maxima()
    {
        var file = Path.GetTempFileName();
        try
        {
            using (var input = System.IO.File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "Fixtures/E0-1819.csv.gz")))
            using (var gzip = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress))
            using (var output = System.IO.File.Create(file)) gzip.CopyTo(output);
            var reader = new ResultReader(); var rows = reader.ReadToList(file).Cast<Result>().ToList();
            Assert.That(rows.Count, Is.EqualTo(380)); Assert.That(reader.Diagnostics, Is.Empty);
            Assert.That(rows.All(x => x.SourcePrices.Single(p => p.Columns == "BbAv").Home == x.HomeOddsAverage), Is.True);
            Assert.That(rows.Count(x => { var max=x.SourcePrices.Single(p=>p.Columns=="BbMx"); return x.HomeOddsAverage==max.Home && x.DrawOddsAverage==max.Draw && x.AwayOddsAverage==max.Away; }), Is.Zero);
        }
        finally { System.IO.File.Delete(file); }
    }
    [Test]
    public void Postponed_season_identity_uses_the_source_url_instead_of_calendar_year()
    {
        var (_, rows) = Parse("Div,Date,HomeTeam,AwayTeam,FTHG,FTAG,FTR\nE0,01/08/2020,A,B,1,0,H\nE0,01/11/2020,A,B,1,0,H\n");
        var hash = rows[0].SourceFileHash;
        SourceIdentity.Attach(rows[0], "https://www.football-data.co.uk/mmz4281/1920/E0.csv");
        SourceIdentity.Attach(rows[1], "https://www.football-data.co.uk/mmz4281/2021/E0.csv");
        Assert.That(rows[0].SourceSeason, Is.EqualTo("1920"));
        Assert.That(rows[1].SourceSeason, Is.EqualTo("2021"));
        Assert.That(rows[0].SourceMatchId, Is.Not.EqualTo(rows[1].SourceMatchId));
        Assert.That(rows[0].SourceFileHash, Is.EqualTo(hash));
    }
}
