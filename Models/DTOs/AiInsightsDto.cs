namespace BugPredictionBackend.Models.DTOs;

public class AiInsightsDto
{
    // Short human-readable insights produced by the AI engine or heuristics
    public List<string> Insights { get; set; } = new();

    // Optional structured items such as risky modules or recommendations
    public List<RiskyModuleDto> RiskyModules { get; set; } = new();

    // Overall score from 0-100 representing risk or attention needed
    public int Score { get; set; }
}

public class RiskyModuleDto
{
    public string? ModuleName { get; set; }
    public string? Path { get; set; }
    public int Bugs { get; set; }
    public int Vulnerabilities { get; set; }
    public int CodeSmells { get; set; }
    public decimal Coverage { get; set; }
    public string? Recommendation { get; set; }
}
