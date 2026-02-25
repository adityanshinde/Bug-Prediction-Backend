using System.Text.Json.Serialization;

namespace BugPredictionBackend.Models.Sonar;

public class SonarBranchResponseEnt
{
    [JsonPropertyName("branches")]
    public List<SonarBranchEnt> Branches { get; set; } = [];
}

public class SonarBranchEnt
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("isMain")]
    public bool IsMain { get; set; }

    [JsonPropertyName("analysisDate")]
    public string? AnalysisDate { get; set; }

    [JsonPropertyName("status")]
    public SonarBranchStatusEnt? Status { get; set; }

    [JsonPropertyName("commit")]
    public SonarBranchCommitEnt? Commit { get; set; }
}

public class SonarBranchStatusEnt
{
    [JsonPropertyName("qualityGateStatus")]
    public string? QualityGateStatus { get; set; }

    [JsonPropertyName("bugs")]
    public int Bugs { get; set; }

    [JsonPropertyName("vulnerabilities")]
    public int Vulnerabilities { get; set; }

    [JsonPropertyName("codeSmells")]
    public int CodeSmells { get; set; }
}

public class SonarBranchCommitEnt
{
    [JsonPropertyName("sha")]
    public string? Sha { get; set; }

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
