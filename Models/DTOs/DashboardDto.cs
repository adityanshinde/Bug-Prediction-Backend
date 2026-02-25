namespace BugPredictionBackend.Models.DTOs;

public class DashboardDto
{
    public DashboardKpisDto Kpis { get; set; } = new();
    public IssueDistributionDto IssueDistribution { get; set; } = new();
    public SeverityDistributionDto SeverityDistribution { get; set; } = new();
    public List<RecentScanDto> RecentScans { get; set; } = [];
}

public class DashboardKpisDto
{
    public int Bugs { get; set; }
    public int Vulnerabilities { get; set; }
    public int CodeSmells { get; set; }
    public decimal Coverage { get; set; }
    public decimal Duplication { get; set; }
    public string? SecurityRating { get; set; }
    public string? ReliabilityRating { get; set; }
    public string? MaintainabilityRating { get; set; }
    public string QualityGate { get; set; } = string.Empty;
}

public class IssueDistributionDto
{
    public int Bugs { get; set; }
    public int Vulnerabilities { get; set; }
    public int CodeSmells { get; set; }
}

public class SeverityDistributionDto
{
    public int Blocker { get; set; }
    public int Critical { get; set; }
    public int Major { get; set; }
    public int Minor { get; set; }
    public int Info { get; set; }
}

public class RecentScanDto
{
    public DateTime ScanDate { get; set; }
    public string? Branch { get; set; }
    public string? Commit { get; set; }
    public string? QualityGate { get; set; }
}
