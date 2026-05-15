using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagementAPI.DTOs;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Services;
using System.Security.Claims;
using System.Text.Json;

namespace ProjectManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly ProjectService _projectService;
        private readonly UserService _userService;

        public ProjectsController(ProjectService projectService, UserService userService)
        {
            _projectService = projectService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            var projects = await _projectService.GetAllProjects();
            ActionResult<IEnumerable<Project>> result = Ok(projects);

            return result;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DetailedProjectDTO>> GetProject(int id)
        {
            ActionResult<DetailedProjectDTO> result;
            var project = await _projectService.GetDetailedProject(id);

            if (project == null)
            {
                result = NotFound();
            }
            else
            {
                result = Ok(project);
            }

            return result;
        }

        [Authorize(Roles = "Teacher")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, UpdateProjectDto dto)
        {
            IActionResult result;
            var project = await _projectService.UpdateProject(id, dto.Name, dto.Description);

            if (project == null)
            {
                result = NotFound();
            }
            else
            {
                result = NoContent();
            }

            return result;
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public async Task<ActionResult<Project>> PostProject([FromBody] JsonElement json)
        {
            ActionResult<Project> result;
            string? name = json.TryGetProperty("name", out var nameProperty) ? nameProperty.GetString() : null;
            string? description = json.TryGetProperty("description", out var descriptionProperty)
                ? descriptionProperty.GetString()
                : null;

            if (name == null || description == null)
            {
                result = BadRequest("Name and description are required.");
            }
            else
            {
                int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                Project project = await _projectService.CreateProject(name, description, teacherId);
                result = Ok(project);
            }

            return result;
        }

        [Authorize(Roles = "Teacher")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            IActionResult result;

            if (!_projectService.ProjectExists(id))
            {
                result = NotFound();
            }
            else
            {
                int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                if (!_projectService.ProjectBelongsToTeacher(id, teacherId))
                {
                    result = Forbid();
                }
                else
                {
                    var project = await _projectService.DeleteProject(id);
                    result = Ok(project);
                }
            }

            return result;
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost("link/{projectId}/students")]
        public async Task<ActionResult> AddStudentToProject(int projectId, StudentProjectDTO dto)
        {
            ActionResult result;
            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (!_projectService.ProjectExists(projectId))
            {
                result = NotFound("Project not found");
            }
            else if (!_projectService.ProjectBelongsToTeacher(projectId, teacherId))
            {
                result = Forbid();
            }
            else if (!_userService.UserExists(dto.StudentId))
            {
                result = NotFound("Student not found");
            }
            else if (_projectService.StudentBelongsToProject(projectId, dto.StudentId))
            {
                result = BadRequest("Student already in Project");
            }
            else
            {
                var relation = await _projectService.AddStudentToProject(projectId, dto.StudentId, dto.Role);
                result = Ok(relation);
            }

            return result;
        }

        [Authorize(Roles = "Teacher")]
        [HttpDelete("{projectId}/unlink/{studentId}")]
        public async Task<ActionResult> DeleteStudentFromProject(int projectId, int studentId)
        {
            ActionResult result;
            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (!_projectService.ProjectExists(projectId))
            {
                result = NotFound("Project not found");
            }
            else if (!_userService.UserExists(studentId))
            {
                result = NotFound("Student not found");
            }
            else if (!_projectService.ProjectBelongsToTeacher(projectId, teacherId))
            {
                result = Forbid();
            }
            else if (!_projectService.StudentBelongsToProject(projectId, studentId))
            {
                result = NotFound("Student not in project.");
            }
            else
            {
                var relation = await _projectService.DeleteStudentFromProject(projectId, studentId);
                result = Ok(relation);
            }

            return result;
        }
    }
}
