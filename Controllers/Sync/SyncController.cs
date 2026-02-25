using BugPredictionBackend.Services.Sync;
using Microsoft.AspNetCore.Mvc;

namespace BugPredictionBackend.Controllers.Sync;

[ApiController]
[Route("api/sync")]
[Tags("Sonar to Database")]
public class SyncController(SyncService syncService, ILogger<SyncController> logger) : ControllerBase
{
    [HttpPost("{projectKey}")]
    public async Task<IActionResult> SyncProject(string projectKey)
    {
        logger.LogInformation("Manual sync triggered for: {ProjectKey}", projectKey);
        await syncService.SyncSingleProjectAsync(projectKey);
        return Ok(new { message = $"Sync completed for project: {projectKey}" });
    }

    [HttpPost("all")]
    public async Task<IActionResult> SyncAll()
    {
        logger.LogInformation("Manual full sync triggered.");
        await syncService.SyncAllProjectsAsync();
        return Ok(new { message = "Full sync completed." });
    }
}
