using System.Text.Json.Serialization;

namespace BugPredictionBackend.Models.Sonar;

public class SonarIssueResponseEnt
{
    [JsonPropertyName("facets")]
    public List<SonarFacetEnt> Facets { get; set; } = [];
}

public class SonarFacetEnt
{
    [JsonPropertyName("property")]
    public string Property { get; set; } = string.Empty;

    [JsonPropertyName("values")]
    public List<SonarFacetValueEnt> Values { get; set; } = [];
}

public class SonarFacetValueEnt
{
    [JsonPropertyName("val")]
    public string Val { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
