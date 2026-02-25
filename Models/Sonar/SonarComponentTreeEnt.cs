using System.Text.Json.Serialization;

namespace BugPredictionBackend.Models.Sonar;

public class SonarComponentTreeResponseEnt
{
    [JsonPropertyName("components")]
    public List<SonarComponentEnt> Components { get; set; } = [];
}

public class SonarComponentEnt
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("qualifier")]
    public string? Qualifier { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("measures")]
    public List<SonarMeasureEnt> Measures { get; set; } = [];
}
