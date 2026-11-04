namespace PowerLinesDataService.Models;

public class SourceRecord
{
    public string SourceUrl { get; set; }
    public string SourceSeason { get; set; }
    public string SourceFileHash { get; set; }
    public string SourceRowId { get; set; }
    public string SourceMatchId { get; set; }
    public string PriceKind { get; set; } = "Unknown";
    public string PriceCollection { get; set; } = "Unknown timestamp";
    public DateTime? QuoteObservedAt { get; set; }
    public List<SourcePrice> SourcePrices { get; set; } = [];
}

public record SourcePrice(string Columns, string Kind, decimal Home, decimal Draw, decimal Away);
