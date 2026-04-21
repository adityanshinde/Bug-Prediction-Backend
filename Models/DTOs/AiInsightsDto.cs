namespace BugPredictionBackend.Models.DTOs;

public class AiInsightsDto
{
    // Short human-readable insights produced by the AI engine or heuristics
    public List<string> Insights { get; set; } = new();

    // Optional structured items such as risky modules or recommendations
    public List<RiskyModuleDto> RiskyModules { get; set; } = new();

    // Overall score from 0-100 representing risk or attention needed
    public int Score { get; set; }

    // Structured output for rendering richer AI recommendations in the same endpoint response
    public AiStructuredInsightsDto Structured { get; set; } = new();
}

public class AiStructuredInsightsDto
{
    public string ExecutiveSummary { get; set; } = string.Empty;
    public List<AiRiskDriverDto> RiskDrivers { get; set; } = new();
    public List<AiActionPlanDto> ActionPlan { get; set; } = new();
    public List<string> QuickWins { get; set; } = new();
    public List<string> WatchItems { get; set; } = new();
    public string Confidence { get; set; } = "Medium";
    public List<string> Assumptions { get; set; } = new();
}

public class AiRiskDriverDto
{
    public string Title { get; set; } = string.Empty;
    public string Evidence { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
}

public class AiActionPlanDto
{
    public string Priority { get; set; } = "P2";
    public string Action { get; set; } = string.Empty;
    public string OwnerType { get; set; } = "Backend";
    public string Effort { get; set; } = "Medium";
    public string ExpectedImpact { get; set; } = string.Empty;
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
