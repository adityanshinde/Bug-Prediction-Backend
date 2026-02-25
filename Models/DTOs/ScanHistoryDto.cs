namespace BugPredictionBackend.Models.DTOs;

public class ScanHistoryDto
{
    public DateTime ScanDate { get; set; }
    public string? Branch { get; set; }
    public string? CommitId { get; set; }
    public string? QualityGateStatus { get; set; }
    public int Bugs { get; set; }
    public int Vulnerabilities { get; set; }
    public int CodeSmells { get; set; }
    public decimal Coverage { get; set; }
    public decimal Duplication { get; set; }
}
