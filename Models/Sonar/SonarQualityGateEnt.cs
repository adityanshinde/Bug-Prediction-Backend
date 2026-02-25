using System.Text.Json.Serialization;

namespace BugPredictionBackend.Models.Sonar;

public class SonarQualityGateResponseEnt
{
    [JsonPropertyName("projectStatus")]
    public SonarQualityGateStatusEnt? ProjectStatus { get; set; }
}

public class SonarQualityGateStatusEnt
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("conditions")]
    public List<SonarQualityGateConditionEnt> Conditions { get; set; } = [];

    [JsonPropertyName("ignoredConditions")]
    public bool IgnoredConditions { get; set; }
}

public class SonarQualityGateConditionEnt
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("metricKey")]
    public string MetricKey { get; set; } = string.Empty;

    [JsonPropertyName("comparator")]
    public string? Comparator { get; set; }

    [JsonPropertyName("errorThreshold")]
    public string? ErrorThreshold { get; set; }

    [JsonPropertyName("actualValue")]
    public string? ActualValue { get; set; }
}
