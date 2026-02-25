using BugPredictionBackend.Models.DTOs;
using BugPredictionBackend.Models.Entities;
using BugPredictionBackend.Repositories.Read;
using BugPredictionBackend.Repositories.Sync;

namespace BugPredictionBackend.Services.Read;

public class QualityGateService(
    QualityGateReadRepository qualityGateReadRepository,
    QAReadRepository qaReadRepository,
    SnapshotRepository snapshotRepository)
{
    public async Task<QualityGateDto?> GetQualityGateAsync(int projectId)
    {
        SnapshotEnt? snapshot = await snapshotRepository.GetLatestAsync(projectId);
        if (snapshot is null) return null;

        List<QualityGateConditionDto> conditions = await qaReadRepository.GetConditionsBySnapshotAsync(snapshot.Id);
        List<QualityGateHistoryDto> history = await qualityGateReadRepository.GetHistoryAsync(projectId);

        return new QualityGateDto
        {
            CurrentStatus  = snapshot.QualityGateStatus ?? "UNKNOWN",
            GateConditions = conditions,
            History        = history
        };
    }
}
