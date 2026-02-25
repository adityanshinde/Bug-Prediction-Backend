namespace BugPredictionBackend.Models.Entities;

public class SeverityDistributionEnt
{
    public int Id { get; set; }
    public int SnapshotId { get; set; }
    public int Blocker { get; set; }
    public int Critical { get; set; }
    public int Major { get; set; }
    public int Minor { get; set; }
    public int Info { get; set; }
}
