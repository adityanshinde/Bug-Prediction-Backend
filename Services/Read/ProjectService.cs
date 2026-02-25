using BugPredictionBackend.Models.DTOs;
using BugPredictionBackend.Models.Entities;
using BugPredictionBackend.Repositories.Read;

namespace BugPredictionBackend.Services.Read;

public class ProjectService(ProjectReadRepository projectReadRepository)
{
    public async Task<List<ProjectListDto>> GetAllAsync()
    {
        List<ProjectEnt> projects = await projectReadRepository.GetAllAsync();
        return projects.Select(p => new ProjectListDto
        {
            ProjectId    = p.Id,
            ProjectKey   = p.ProjectKey,
            Name         = p.Name,
            Organization = p.Organization,
            Visibility   = p.Visibility,
            LastScanDate = p.LastAnalysisDate
        }).ToList();
    }

    public async Task<bool> ExistsAsync(int projectId)
        => await projectReadRepository.ExistsAsync(projectId);
}
