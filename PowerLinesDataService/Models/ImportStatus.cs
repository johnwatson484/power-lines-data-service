namespace PowerLinesDataService.Models;

public class ImportStatus
{
    public string RecordType { get; set; } = "ImportStatus";
    public string SourceUrl { get; set; }
    public string Division { get; set; }
    public string SourceSeason { get; set; }
    public string SourceFileHash { get; set; }
    public int RowCount { get; set; }
    public DateTime CompletedAt { get; set; }
    public string Status { get; set; } = "Completed";
}
