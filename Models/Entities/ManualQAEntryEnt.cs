namespace BugPredictionBackend.Models.Entities;

public class ManualQAEntryEnt
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ReportedBy { get; set; }
    public DateTime EntryDate { get; set; }
}
