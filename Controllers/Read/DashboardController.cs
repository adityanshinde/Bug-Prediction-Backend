using BugPredictionBackend.Models.DTOs;
using BugPredictionBackend.Services.Read;
using Microsoft.AspNetCore.Mvc;

namespace BugPredictionBackend.Controllers.Read;

[ApiController]
[Route("api/projects/{projectId:int}")]
[Tags("Database to Frontend")]
public class DashboardController(DashboardService dashboardService, ProjectService projectService) : ControllerBase
{
    [HttpGet("header")]
    public async Task<IActionResult> GetHeader(int projectId)
    {
        bool exists = await projectService.ExistsAsync(projectId);
        if (!exists) return NotFound(new { message = $"Project {projectId} not found." });

        HeaderDto? header = await dashboardService.GetHeaderAsync(projectId);
        return header is null ? NotFound(new { message = "No scan data found." }) : Ok(header);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(int projectId)
    {
        bool exists = await projectService.ExistsAsync(projectId);
        if (!exists) return NotFound(new { message = $"Project {projectId} not found." });

        DashboardDto? dashboard = await dashboardService.GetDashboardAsync(projectId);
        return dashboard is null ? NotFound(new { message = "No snapshot data found." }) : Ok(dashboard);
    }
}
