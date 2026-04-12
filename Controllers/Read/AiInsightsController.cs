using BugPredictionBackend.Models.DTOs;
using BugPredictionBackend.Services.Read;
using Microsoft.AspNetCore.Mvc;

namespace BugPredictionBackend.Controllers.Read;

[ApiController]
[Route("api/projects/{projectId:int}")]
[Tags("Database to Frontend")]
public class AiInsightsController(AiInsightsService aiInsightsService, ProjectService projectService) : ControllerBase
{
    [HttpGet("ai/insights")]
    public async Task<IActionResult> GetAiInsights(int projectId)
    {
        bool exists = await projectService.ExistsAsync(projectId);
        if (!exists) return NotFound(new { message = $"Project {projectId} not found." });

        AiInsightsDto? insights = await aiInsightsService.GetInsightsAsync(projectId);
        return insights is null ? NotFound(new { message = "No snapshot data found." }) : Ok(insights);
    }
}
