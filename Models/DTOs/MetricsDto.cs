namespace BugPredictionBackend.Models.DTOs;

public class MetricsDto
{
    public MetricsKpisDto Kpis { get; set; } = new();
    public List<CoverageTrendPointDto> CoverageTrend { get; set; } = [];
    public List<BugsVsVulnerabilitiesPointDto> BugsVsVulnerabilities { get; set; } = [];
    public List<ModuleMetricDto> ModuleMetrics { get; set; } = [];
}

public class MetricsKpisDto
{
    public int Bugs { get; set; }
    public int CodeSmells { get; set; }
    public decimal Coverage { get; set; }
    public decimal Duplication { get; set; }
}

public class CoverageTrendPointDto
{
    public DateTime Date { get; set; }
    public decimal Coverage { get; set; }
}

public class BugsVsVulnerabilitiesPointDto
{
    public DateTime Date { get; set; }
    public int Bugs { get; set; }
    public int Vulnerabilities { get; set; }
}

public class ModuleMetricDto
{
    public string ModuleName { get; set; } = string.Empty;
    public string? Qualifier { get; set; }
    public string? Language { get; set; }
    public int Bugs { get; set; }
    public int Vulnerabilities { get; set; }
    public int CodeSmells { get; set; }
    public decimal Coverage { get; set; }
    public decimal Duplication { get; set; }
    public int Complexity { get; set; }
    public int LinesOfCode { get; set; }
}
