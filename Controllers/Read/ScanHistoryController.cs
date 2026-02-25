using BugPredictionBackend.Models.DTOs;
using BugPredictionBackend.Services.Read;
using Microsoft.AspNetCore.Mvc;

namespace BugPredictionBackend.Controllers.Read;

[ApiController]
[Route("api/projects/{projectId:int}")]
[Tags("Database to Frontend")]
public class ScanHistoryController(ScanHistoryService scanHistoryService, ProjectService projectService) : ControllerBase
{
    [HttpGet("scan-history")]
    public async Task<IActionResult> GetScanHistory(int projectId)
    {
        bool exists = await projectService.ExistsAsync(projectId);
        if (!exists) return NotFound(new { message = $"Project {projectId} not found." });

        List<ScanHistoryDto> history = await scanHistoryService.GetByProjectAsync(projectId);
        return Ok(history);
    }
}
