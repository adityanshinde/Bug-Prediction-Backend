namespace BugPredictionBackend.Models.DTOs;

public class QualityGateDto
{
    public string CurrentStatus { get; set; } = string.Empty;
    public List<QualityGateConditionDto> GateConditions { get; set; } = [];
    public List<QualityGateHistoryDto> History { get; set; } = [];
}

public class QualityGateConditionDto
{
    public string Metric { get; set; } = string.Empty;
    public string? Condition { get; set; }
    public string? ActualValue { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class QualityGateHistoryDto
{
    public DateTime Date { get; set; }
    public string? Branch { get; set; }
    public string? Status { get; set; }
    public string? CommitId { get; set; }
}
