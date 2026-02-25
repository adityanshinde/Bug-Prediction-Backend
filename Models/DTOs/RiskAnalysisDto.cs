namespace BugPredictionBackend.Models.DTOs;

public class RiskAnalysisDto
{
    public List<ModuleRiskDto> ModuleDistribution { get; set; } = [];
    public List<HighRiskModuleDto> HighRiskModules { get; set; } = [];
}

public class ModuleRiskDto
{
    public string ModuleName { get; set; } = string.Empty;
    public int Bugs { get; set; }
    public int Vulnerabilities { get; set; }
    public decimal Coverage { get; set; }
    public decimal Duplication { get; set; }
    public int Complexity { get; set; }
}

public class HighRiskModuleDto
{
    public string ModuleName { get; set; } = string.Empty;
    public string? Language { get; set; }
    public int Bugs { get; set; }
    public int Vulnerabilities { get; set; }
    public decimal Coverage { get; set; }
    public decimal Duplication { get; set; }
    public int Complexity { get; set; }
    public int LinesOfCode { get; set; }
}
