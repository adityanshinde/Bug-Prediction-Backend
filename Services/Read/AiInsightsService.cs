using BugPredictionBackend.Models.DTOs;
using BugPredictionBackend.Models.Entities;
using BugPredictionBackend.Repositories.Read;
using BugPredictionBackend.Repositories.Sync;

namespace BugPredictionBackend.Services.Read;

public class AiInsightsService(
    SnapshotRepository snapshotRepository,
    MetricsReadRepository metricsReadRepository,
    ModuleRepository moduleRepository,
    SeverityRepository severityRepository)
{
    public async Task<AiInsightsDto?> GetInsightsAsync(int projectId)
    {
        SnapshotEnt? snapshot = await snapshotRepository.GetLatestAsync(projectId);
        if (snapshot is null) return null;

        AiInsightsDto result = new();

        // Basic KPIs
        MetricsKpisDto? kpis = await metricsReadRepository.GetKpisAsync(projectId);
        var severity = await severityRepository.GetBySnapshotAsync(snapshot.Id);
        var modules = await moduleRepository.GetBySnapshotAsync(snapshot.Id);

        // Insight: Coverage status
        if (kpis is not null)
        {
            if (kpis.Coverage < 60)
                result.Insights.Add($"Coverage is low ({kpis.Coverage}%). Consider adding tests to increase coverage.");
            else if (kpis.Coverage < 80)
                result.Insights.Add($"Coverage is moderate ({kpis.Coverage}%). Aim for >80% for better confidence.");
            else
                result.Insights.Add($"Coverage looks healthy ({kpis.Coverage}%).");
        }

        // Insight: Severity
        if (severity is not null)
        {
            if (severity.Critical > 0 || severity.Blocker > 0)
                result.Insights.Add($"High severity issues detected: {severity.Blocker} blocker(s), {severity.Critical} critical(s). Prioritize fixes.");
            else if (severity.Major > 50)
                result.Insights.Add($"Large number of major issues ({severity.Major}). Review recent changes for regressions.");
        }

        // Insight: Top risky modules
        var topRisky = modules
            .OrderByDescending(m => (m.Bugs + m.Vulnerabilities + m.CodeSmells))
            .Take(5)
            .ToList();

        foreach (var m in topRisky)
        {
            result.RiskyModules.Add(new RiskyModuleDto
            {
                ModuleName = m.ModuleName,
                Path = m.Path,
                Bugs = m.Bugs,
                Vulnerabilities = m.Vulnerabilities,
                CodeSmells = m.CodeSmells,
                Coverage = m.Coverage,
                Recommendation = GenerateRecommendation(m)
            });
        }

        // Simple score calculation
        int score = 100;
        if (kpis is not null) score -= (int)Math.Clamp((100 - (double)kpis.Coverage) / 2, 0, 60);
        if (severity is not null) score -= (severity.Critical * 5 + severity.Blocker * 10 + severity.Major);
        result.Score = Math.Max(0, score);

        // Add summary insight
        result.Insights.Insert(0, $"Overall project health score: {result.Score}/100");

        return result;
    }

    private static string GenerateRecommendation(BugPredictionBackend.Models.Entities.ModuleMetricEnt m)
    {
        List<string> recs = new();
        if (m.Coverage < 70) recs.Add("Increase unit tests for this module");
        if (m.Bugs > 5) recs.Add("Investigate recent bug reports and root causes");
        if (m.Vulnerabilities > 0) recs.Add("Address security vulnerabilities immediately");
        if (m.CodeSmells > 20) recs.Add("Refactor to reduce code smells and complexity");
        return string.Join("; ", recs);
    }
}
