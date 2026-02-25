using BugPredictionBackend.Models.DTOs;
using BugPredictionBackend.Services.Read;
using Microsoft.AspNetCore.Mvc;

namespace BugPredictionBackend.Controllers.Read;

[ApiController]
[Route("api/projects")]
[Tags("Database to Frontend")]
public class ProjectsController(ProjectService projectService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        List<ProjectListDto> projects = await projectService.GetAllAsync();
        return Ok(projects);
    }
}
