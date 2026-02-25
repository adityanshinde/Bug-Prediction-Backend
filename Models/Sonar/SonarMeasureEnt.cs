using System.Text.Json.Serialization;

namespace BugPredictionBackend.Models.Sonar;

public class SonarMeasureResponseEnt
{
    [JsonPropertyName("component")]
    public SonarMeasureComponentEnt? Component { get; set; }
}

public class SonarMeasureComponentEnt
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("measures")]
    public List<SonarMeasureEnt> Measures { get; set; } = [];
}

public class SonarMeasureEnt
{
    [JsonPropertyName("metric")]
    public string Metric { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
