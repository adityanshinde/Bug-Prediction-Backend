namespace BugPredictionBackend.Models.DTOs;

public class HeaderDto
{
    public string ProjectName { get; set; } = string.Empty;
    public string Branch { get; set; } = string.Empty;
    public DateTime? LastScanDate { get; set; }
    public string QualityGateStatus { get; set; } = string.Empty;
    public string? CommitId { get; set; }
}
