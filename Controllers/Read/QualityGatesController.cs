using BugPredictionBackend.Models.DTOs;
using BugPredictionBackend.Services.Read;
using Microsoft.AspNetCore.Mvc;

namespace BugPredictionBackend.Controllers.Read;

[ApiController]
[Route("api/projects/{projectId:int}")]
[Tags("Database to Frontend")]
public class QualityGatesController(QualityGateService qualityGateService, ProjectService projectService) : ControllerBase
{
    [HttpGet("quality-gates")]
    public async Task<IActionResult> GetQualityGates(int projectId)
    {
        bool exists = await projectService.ExistsAsync(projectId);
        if (!exists) return NotFound(new { message = $"Project {projectId} not found." });

        QualityGateDto? result = await qualityGateService.GetQualityGateAsync(projectId);
        return result is null ? NotFound(new { message = "No quality gate data found." }) : Ok(result);
    }
}
