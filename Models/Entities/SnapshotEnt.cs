namespace BugPredictionBackend.Models.Entities;

public class SnapshotEnt
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int? BranchId { get; set; }
    public DateTime ScanDate { get; set; }
    public string? CommitId { get; set; }
    public int Bugs { get; set; }
    public int Vulnerabilities { get; set; }
    public int CodeSmells { get; set; }
    public decimal Coverage { get; set; }
    public decimal Duplication { get; set; }
    public string? SecurityRating { get; set; }
    public string? ReliabilityRating { get; set; }
    public string? MaintainabilityRating { get; set; }
    public string? QualityGateStatus { get; set; }
}
