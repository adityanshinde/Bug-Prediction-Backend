namespace BugPredictionBackend.Models.DTOs;

public class ProjectListDto
{
    public int ProjectId { get; set; }
    public string ProjectKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Organization { get; set; }
    public string? Visibility { get; set; }
    public DateTime? LastScanDate { get; set; }
}
