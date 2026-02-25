using BugPredictionBackend.Models.DTOs;
using BugPredictionBackend.Models.Entities;
using BugPredictionBackend.Repositories.Read;
using BugPredictionBackend.Repositories.Sync;

namespace BugPredictionBackend.Services.Read;

public class DashboardService(
    DashboardReadRepository dashboardReadRepository,
    SnapshotRepository snapshotRepository,
    SeverityRepository severityRepository)
{
    public async Task<DashboardDto?> GetDashboardAsync(int projectId)
    {
        SnapshotEnt? snapshot = await snapshotRepository.GetLatestAsync(projectId);
        if (snapshot is null) return null;

        SeverityDistributionEnt? severity = await severityRepository.GetBySnapshotAsync(snapshot.Id);
        List<RecentScanDto> recentScans = await dashboardReadRepository.GetRecentScansAsync(projectId);

        return new DashboardDto
        {
            Kpis = new DashboardKpisDto
            {
                Bugs                  = snapshot.Bugs,
                Vulnerabilities       = snapshot.Vulnerabilities,
                CodeSmells            = snapshot.CodeSmells,
                Coverage              = snapshot.Coverage,
                Duplication           = snapshot.Duplication,
                SecurityRating        = snapshot.SecurityRating,
                ReliabilityRating     = snapshot.ReliabilityRating,
                MaintainabilityRating = snapshot.MaintainabilityRating,
                QualityGate           = snapshot.QualityGateStatus ?? "UNKNOWN"
            },
            IssueDistribution = new IssueDistributionDto
            {
                Bugs            = snapshot.Bugs,
                Vulnerabilities = snapshot.Vulnerabilities,
                CodeSmells      = snapshot.CodeSmells
            },
            SeverityDistribution = new SeverityDistributionDto
            {
                Blocker  = severity?.Blocker  ?? 0,
                Critical = severity?.Critical ?? 0,
                Major    = severity?.Major    ?? 0,
                Minor    = severity?.Minor    ?? 0,
                Info     = severity?.Info     ?? 0
            },
            RecentScans = recentScans
        };
    }

    public async Task<HeaderDto?> GetHeaderAsync(int projectId)
        => await dashboardReadRepository.GetHeaderAsync(projectId);
}
