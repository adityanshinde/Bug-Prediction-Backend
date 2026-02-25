using BugPredictionBackend.Models.DTOs;
using BugPredictionBackend.Services.Read;
using Microsoft.AspNetCore.Mvc;

namespace BugPredictionBackend.Controllers.Read;

[ApiController]
[Route("api/projects/{projectId:int}")]
[Tags("Database to Frontend")]
public class QAController(QAService qaService, ProjectService projectService) : ControllerBase
{
    [HttpPost("qa-entries")]
    public async Task<IActionResult> SubmitEntry(int projectId, [FromBody] QAEntryRequestDto request)
    {
        bool exists = await projectService.ExistsAsync(projectId);
        if (!exists) return NotFound(new { message = $"Project {projectId} not found." });

        QAEntryResponseDto result = await qaService.SubmitEntryAsync(projectId, request);
        return Ok(result);
    }

    [HttpGet("qa-entries")]
    public async Task<IActionResult> GetEntries(int projectId)
    {
        bool exists = await projectService.ExistsAsync(projectId);
        if (!exists) return NotFound(new { message = $"Project {projectId} not found." });

        QASummaryDto summary = await qaService.GetSummaryAsync(projectId);
        return Ok(summary);
    }
}
