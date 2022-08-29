using System;

namespace PowerLinesDataService.Models;

public class Fixture
{
    public string Division { get; set; }
    public DateTime Date { get; set; }
    public string HomeTeam { get; set; }
    public string AwayTeam { get; set; }
    public decimal HomeOddsAverage { get; set; }
    public decimal DrawOddsAverage { get; set; }
    public decimal AwayOddsAverage { get; set; }
}
