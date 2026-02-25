namespace BugPredictionBackend.Models.Entities;

public class ModuleMetricEnt
{
    public int Id { get; set; }
    public int SnapshotId { get; set; }
    public string? ModuleName { get; set; }
    public string? Qualifier { get; set; }
    public string? Path { get; set; }
    public string? Language { get; set; }
    public int Bugs { get; set; }
    public int Vulnerabilities { get; set; }
    public int CodeSmells { get; set; }
    public decimal Coverage { get; set; }
    public decimal Duplication { get; set; }
    public int Complexity { get; set; }
    public int Ncloc { get; set; }
}
