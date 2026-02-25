using BugPredictionBackend.Models.DTOs;
using BugPredictionBackend.Services.Read;
using Microsoft.AspNetCore.Mvc;

namespace BugPredictionBackend.Controllers.Read;

[ApiController]
[Route("api/projects/{projectId:int}")]
[Tags("Database to Frontend")]
public class MetricsController(MetricsService metricsService, ProjectService projectService) : ControllerBase
{
    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics(int projectId)
    {
        bool exists = await projectService.ExistsAsync(projectId);
        if (!exists) return NotFound(new { message = $"Project {projectId} not found." });

        MetricsDto? metrics = await metricsService.GetMetricsAsync(projectId);
        return metrics is null ? NotFound(new { message = "No metrics data found." }) : Ok(metrics);
    }

    [HttpGet("risk-analysis")]
    public async Task<IActionResult> GetRiskAnalysis(int projectId)
    {
        bool exists = await projectService.ExistsAsync(projectId);
        if (!exists) return NotFound(new { message = $"Project {projectId} not found." });

        RiskAnalysisDto? risk = await metricsService.GetRiskAnalysisAsync(projectId);
        return risk is null ? NotFound(new { message = "No module data found." }) : Ok(risk);
    }
}
