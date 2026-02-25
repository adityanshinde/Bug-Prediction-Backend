namespace BugPredictionBackend.Models.Entities;

public class BranchEnt
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public DateTime? AnalysisDate { get; set; }
}
