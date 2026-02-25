using BugPredictionBackend.Models.DTOs;
using BugPredictionBackend.Models.Entities;
using BugPredictionBackend.Repositories.Read;
using BugPredictionBackend.Repositories.Sync;

namespace BugPredictionBackend.Services.Read;

public class MetricsService(
    MetricsReadRepository metricsReadRepository,
    SnapshotRepository snapshotRepository,
    ModuleRepository moduleRepository)
{
    public async Task<MetricsDto?> GetMetricsAsync(int projectId)
    {
        SnapshotEnt? snapshot = await snapshotRepository.GetLatestAsync(projectId);
        if (snapshot is null) return null;

        MetricsKpisDto? kpis = await metricsReadRepository.GetKpisAsync(projectId);
        List<CoverageTrendPointDto> coverageTrend = await metricsReadRepository.GetCoverageTrendAsync(projectId);
        List<BugsVsVulnerabilitiesPointDto> bugsVsVuln = await metricsReadRepository.GetBugsVsVulnerabilitiesAsync(projectId);
        List<ModuleMetricEnt> modules = await moduleRepository.GetBySnapshotAsync(snapshot.Id);

        return new MetricsDto
        {
            Kpis = kpis ?? new MetricsKpisDto(),
            CoverageTrend = coverageTrend,
            BugsVsVulnerabilities = bugsVsVuln,
            ModuleMetrics = modules.Select(m => new ModuleMetricDto
            {
                ModuleName      = m.ModuleName ?? string.Empty,
                Qualifier       = m.Qualifier,
                Language        = m.Language,
                Bugs            = m.Bugs,
                Vulnerabilities = m.Vulnerabilities,
                CodeSmells      = m.CodeSmells,
                Coverage        = m.Coverage,
                Duplication     = m.Duplication,
                Complexity      = m.Complexity,
                LinesOfCode     = m.Ncloc
            }).ToList()
        };
    }

    public async Task<RiskAnalysisDto?> GetRiskAnalysisAsync(int projectId)
    {
        SnapshotEnt? snapshot = await snapshotRepository.GetLatestAsync(projectId);
        if (snapshot is null) return null;

        List<ModuleMetricEnt> modules = await moduleRepository.GetBySnapshotAsync(snapshot.Id);

        return new RiskAnalysisDto
        {
            ModuleDistribution = modules.Select(m => new ModuleRiskDto
            {
                ModuleName      = m.ModuleName ?? string.Empty,
                Bugs            = m.Bugs,
                Vulnerabilities = m.Vulnerabilities,
                Coverage        = m.Coverage,
                Duplication     = m.Duplication,
                Complexity      = m.Complexity
            }).ToList(),
            HighRiskModules = modules
                .Where(m => m.Bugs > 0 || m.Vulnerabilities > 0)
                .OrderByDescending(m => m.Bugs + m.Vulnerabilities)
                .Select(m => new HighRiskModuleDto
                {
                    ModuleName      = m.ModuleName ?? string.Empty,
                    Language        = m.Language,
                    Bugs            = m.Bugs,
                    Vulnerabilities = m.Vulnerabilities,
                    Coverage        = m.Coverage,
                    Duplication     = m.Duplication,
                    Complexity      = m.Complexity,
                    LinesOfCode     = m.Ncloc
                }).ToList()
        };
    }
}
