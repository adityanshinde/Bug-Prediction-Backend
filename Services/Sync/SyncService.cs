using BugPredictionBackend.Models.Entities;
using BugPredictionBackend.Models.Sonar;
using BugPredictionBackend.Repositories.Sync;
using BugPredictionBackend.Services.Sonar;

namespace BugPredictionBackend.Services.Sync;

public class SyncService(
    SonarApiClient sonarApiClient,
    ProjectRepository projectRepository,
    BranchRepository branchRepository,
    SnapshotRepository snapshotRepository,
    ModuleRepository moduleRepository,
    SeverityRepository severityRepository,
    ILogger<SyncService> logger)
{
    public async Task SyncAllProjectsAsync()
    {
        List<SonarProjectEnt> projects = await sonarApiClient.GetProjectsAsync();
        logger.LogInformation("Sync started. Projects found: {Count}", projects.Count);

        foreach (SonarProjectEnt sonarProject in projects)
        {
            await SyncProjectAsync(sonarProject);
        }

        logger.LogInformation("Sync completed.");
    }

    public async Task SyncSingleProjectAsync(string projectKey)
    {
        List<SonarProjectEnt> projects = await sonarApiClient.GetProjectsAsync();
        SonarProjectEnt? sonarProject = projects.FirstOrDefault(p => p.Key == projectKey);

        if (sonarProject is null)
        {
            logger.LogWarning("Project not found in SonarCloud: {ProjectKey}", projectKey);
            return;
        }

        await SyncProjectAsync(sonarProject);
    }

    private async Task SyncProjectAsync(SonarProjectEnt sonarProject)
    {
        logger.LogInformation("Syncing project: {ProjectKey}", sonarProject.Key);

        ProjectEnt projectEntity = new()
        {
            ProjectKey       = sonarProject.Key,
            Name             = sonarProject.Name,
            Organization     = sonarProject.Organization,
            Visibility       = sonarProject.Visibility,
            LastAnalysisDate = ParseDate(sonarProject.LastAnalysisDate)
        };

        int projectId = await projectRepository.InsertOrUpdateAsync(projectEntity);

        List<SonarBranchEnt> branches = await sonarApiClient.GetBranchesAsync(sonarProject.Key);
        SonarBranchEnt mainBranch = branches.FirstOrDefault(b => b.IsMain) ?? branches.FirstOrDefault() ?? new SonarBranchEnt { Name = "main" };

        BranchEnt branchEntity = new()
        {
            ProjectId    = projectId,
            BranchName   = mainBranch.Name,
            IsMain       = mainBranch.IsMain,
            AnalysisDate = ParseDate(mainBranch.AnalysisDate)
        };

        int branchId = await branchRepository.InsertOrUpdateAsync(branchEntity);

        Dictionary<string, string> metrics = await sonarApiClient.GetMetricsAsync(sonarProject.Key);
        SonarQualityGateResponseEnt? qualityGate = await sonarApiClient.GetQualityGateAsync(sonarProject.Key);

        SnapshotEnt snapshot = new()
        {
            ProjectId             = projectId,
            BranchId              = branchId,
            ScanDate              = ParseDate(mainBranch.AnalysisDate) ?? DateTime.UtcNow,
            CommitId              = mainBranch.Commit?.Sha,
            Bugs                  = ParseInt(metrics, "bugs"),
            Vulnerabilities       = ParseInt(metrics, "vulnerabilities"),
            CodeSmells            = ParseInt(metrics, "code_smells"),
            Coverage              = ParseDecimal(metrics, "coverage"),
            Duplication           = ParseDecimal(metrics, "duplicated_lines_density"),
            SecurityRating        = metrics.GetValueOrDefault("security_rating"),
            ReliabilityRating     = metrics.GetValueOrDefault("reliability_rating"),
            MaintainabilityRating = metrics.GetValueOrDefault("sqale_rating"),
            QualityGateStatus     = MapGateStatus(qualityGate?.ProjectStatus?.Status)
        };

        int snapshotId = await snapshotRepository.InsertAsync(snapshot);

        await SyncSeverityAsync(sonarProject.Key, snapshotId);
        await SyncModulesAsync(sonarProject.Key, snapshotId);

        logger.LogInformation("Project synced: {ProjectKey} | SnapshotId: {SnapshotId}", sonarProject.Key, snapshotId);
    }

    private async Task SyncSeverityAsync(string projectKey, int snapshotId)
    {
        SonarIssueResponseEnt? issueResponse = await sonarApiClient.GetSeverityAsync(projectKey);
        SonarFacetEnt? severityFacet = issueResponse?.Facets.FirstOrDefault(f => f.Property == "severities");

        SeverityDistributionEnt severity = new()
        {
            SnapshotId = snapshotId,
            Blocker    = GetFacetCount(severityFacet, "BLOCKER"),
            Critical   = GetFacetCount(severityFacet, "CRITICAL"),
            Major      = GetFacetCount(severityFacet, "MAJOR"),
            Minor      = GetFacetCount(severityFacet, "MINOR"),
            Info       = GetFacetCount(severityFacet, "INFO")
        };

        await severityRepository.InsertAsync(severity);
    }

    private async Task SyncModulesAsync(string projectKey, int snapshotId)
    {
        List<SonarComponentEnt> components = await sonarApiClient.GetModuleMetricsAsync(projectKey);

        foreach (SonarComponentEnt component in components)
        {
            Dictionary<string, string> m = component.Measures
                .Where(x => x.Value != null)
                .ToDictionary(x => x.Metric, x => x.Value!);

            ModuleMetricEnt module = new()
            {
                SnapshotId      = snapshotId,
                ModuleName      = component.Name ?? component.Key,
                Qualifier       = component.Qualifier,
                Path            = component.Path,
                Language        = component.Language,
                Bugs            = ParseInt(m, "bugs"),
                Vulnerabilities = ParseInt(m, "vulnerabilities"),
                CodeSmells      = ParseInt(m, "code_smells"),
                Coverage        = ParseDecimal(m, "coverage"),
                Duplication     = ParseDecimal(m, "duplicated_lines_density"),
                Complexity      = ParseInt(m, "complexity"),
                Ncloc           = ParseInt(m, "ncloc")
            };

            await moduleRepository.InsertAsync(module);
        }
    }

    private static DateTime? ParseDate(string? raw)
        => DateTime.TryParse(raw, out DateTime dt) ? dt.ToUniversalTime() : null;

    private static int ParseInt(Dictionary<string, string> dict, string key)
        => dict.TryGetValue(key, out string? val) && int.TryParse(val, out int result) ? result : 0;

    private static decimal ParseDecimal(Dictionary<string, string> dict, string key)
        => dict.TryGetValue(key, out string? val) && decimal.TryParse(val, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal result) ? result : 0;

    private static int GetFacetCount(SonarFacetEnt? facet, string value)
        => facet?.Values.FirstOrDefault(v => v.Val == value)?.Count ?? 0;

    private static string MapGateStatus(string? status)
        => status == "OK" ? "PASS" : status == "ERROR" ? "FAIL" : status ?? "UNKNOWN";
}
