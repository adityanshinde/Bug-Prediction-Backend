using BugPredictionBackend.Models.DTOs;
using BugPredictionBackend.Models.Entities;
using BugPredictionBackend.Repositories.Read;
using BugPredictionBackend.Repositories.Sync;

namespace BugPredictionBackend.Services.Read;

public class QAService(
    QAEntryRepository qaEntryRepository,
    QAReadRepository qaReadRepository)
{
    public async Task<QAEntryResponseDto> SubmitEntryAsync(int projectId, QAEntryRequestDto request)
    {
        ManualQAEntryEnt entity = new()
        {
            ProjectId   = projectId,
            ModuleName  = request.ModuleName,
            IssueType   = request.IssueType,
            Severity    = request.Severity,
            Description = request.Description,
            ReportedBy  = request.ReportedBy
        };

        int newId = await qaEntryRepository.InsertAsync(entity);

        return new QAEntryResponseDto
        {
            Id         = newId,
            ProjectId  = projectId,
            ModuleName = request.ModuleName,
            IssueType  = request.IssueType,
            Severity   = request.Severity,
            Description= request.Description,
            ReportedBy = request.ReportedBy,
            EntryDate  = DateTime.UtcNow
        };
    }

    public async Task<QASummaryDto> GetSummaryAsync(int projectId)
    {
        QASummaryDto summary = await qaReadRepository.GetSummaryAsync(projectId);
        summary.Entries = await qaReadRepository.GetEntriesAsync(projectId);
        return summary;
    }
}
