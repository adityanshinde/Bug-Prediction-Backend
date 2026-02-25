namespace BugPredictionBackend.Models.DTOs;

public class QAEntryRequestDto
{
    public string ModuleName { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty;   // Bug | Vulnerability | Code Smell
    public string Severity { get; set; } = string.Empty;    // Critical | High | Medium | Low
    public string? Description { get; set; }
    public string? ReportedBy { get; set; }
}

public class QAEntryResponseDto
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

public class QASummaryDto
{
    public int TotalEntries { get; set; }
    public int BugEntries { get; set; }
    public int VulnerabilityEntries { get; set; }
    public int CodeSmellEntries { get; set; }
    public int CriticalCount { get; set; }
    public int HighCount { get; set; }
    public int MediumCount { get; set; }
    public int LowCount { get; set; }
    public List<QAEntryResponseDto> Entries { get; set; } = [];
}
