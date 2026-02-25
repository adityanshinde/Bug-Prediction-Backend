using System.Text.Json.Serialization;

namespace BugPredictionBackend.Models.Sonar;

public class SonarProjectSearchResponseEnt
{
    [JsonPropertyName("components")]
    public List<SonarProjectEnt> Components { get; set; } = [];
}

public class SonarProjectEnt
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("organization")]
    public string? Organization { get; set; }

    [JsonPropertyName("visibility")]
    public string? Visibility { get; set; }

    [JsonPropertyName("lastAnalysisDate")]
    public string? LastAnalysisDate { get; set; }

    [JsonPropertyName("revision")]
    public string? Revision { get; set; }
}
