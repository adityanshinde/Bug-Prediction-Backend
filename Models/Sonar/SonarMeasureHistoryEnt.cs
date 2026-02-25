using System.Text.Json.Serialization;

namespace BugPredictionBackend.Models.Sonar;

public class SonarMeasureHistoryResponseEnt
{
    [JsonPropertyName("measures")]
    public List<SonarMeasureHistoryEnt> Measures { get; set; } = [];
}

public class SonarMeasureHistoryEnt
{
    [JsonPropertyName("metric")]
    public string Metric { get; set; } = string.Empty;

    [JsonPropertyName("history")]
    public List<SonarHistoryPointEnt> History { get; set; } = [];
}

public class SonarHistoryPointEnt
{
    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
