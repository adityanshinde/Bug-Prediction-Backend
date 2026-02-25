using System.Text.Json.Serialization;

namespace BugPredictionBackend.Models.Sonar;

public class SonarAnalysisResponseEnt
{
    [JsonPropertyName("analyses")]
    public List<SonarAnalysisEnt> Analyses { get; set; } = [];
}

public class SonarAnalysisEnt
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("revision")]
    public string? Revision { get; set; }

    [JsonPropertyName("projectVersion")]
    public string? ProjectVersion { get; set; }

    [JsonPropertyName("events")]
    public List<SonarAnalysisEventEnt> Events { get; set; } = [];
}

public class SonarAnalysisEventEnt
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}
