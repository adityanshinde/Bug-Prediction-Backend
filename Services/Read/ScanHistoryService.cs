using BugPredictionBackend.Models.DTOs;
using BugPredictionBackend.Repositories.Read;

namespace BugPredictionBackend.Services.Read;

public class ScanHistoryService(ScanHistoryReadRepository scanHistoryReadRepository)
{
    public async Task<List<ScanHistoryDto>> GetByProjectAsync(int projectId)
        => await scanHistoryReadRepository.GetByProjectAsync(projectId);
}
