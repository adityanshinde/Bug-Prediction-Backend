namespace BugPredictionBackend.Models.Entities;

public class ProjectEnt
{
    public int Id { get; set; }
    public string ProjectKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Organization { get; set; }
    public string? Visibility { get; set; }
    public DateTime? LastAnalysisDate { get; set; }
    public DateTime CreatedDate { get; set; }
}
