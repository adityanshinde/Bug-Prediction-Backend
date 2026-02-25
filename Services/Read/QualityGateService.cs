using BugPredictionBackend.Models.DTOs;
using BugPredictionBackend.Models.Entities;
using BugPredictionBackend.Repositories.Read;
using BugPredictionBackend.Repositories.Sync;

namespace BugPredictionBackend.Services.Read;

public class QualityGateService(
    QualityGateReadRepository qualityGateReadRepository,
    SnapshotRepository snapshotRepository)
{
    public async Task<QualityGateDto?> GetQualityGateAsync(int projectId)
    {
        SnapshotEnt? snapshot = await snapshotRepository.GetLatestAsync(projectId);
        if (snapshot is null) return null;

        List<QualityGateHistoryDto> history = await qualityGateReadRepository.GetHistoryAsync(projectId);

        return new QualityGateDto
        {
            CurrentStatus  = snapshot.QualityGateStatus ?? "UNKNOWN",
            GateConditions = [],
            History        = history
        };
    }
}
