namespace BugPredictionBackend.Models.Entities;

public class QualityGateConditionEnt
{
    public int Id { get; set; }
    public int SnapshotId { get; set; }
    public string MetricKey { get; set; } = string.Empty;
    public string? Comparator { get; set; }
    public string? ErrorThreshold { get; set; }
    public string? ActualValue { get; set; }
    public string Status { get; set; } = string.Empty;
}
