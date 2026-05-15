using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementAPI.Data;
using ProjectManagementAPI.DTOs;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;


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
            return await _projectService.GetAllProjects();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DetailedProjectDTO>> GetProject(int id)
        {
            ActionResult<DetailedProjectDTO> result = NotFound();

            if (_projectService.ProjectExists(id))
            {
                ActionResult<DetailedProjectDTO> detailedProject = await _projectService.GetDetailedProject(id);

                result = Ok(detailedProject);
            }


            return result;
        }

        [Authorize(Roles = "Teacher")]
        [HttpPut("{id}")]
        public async Task<ActionResult<DetailedProjectDTO>> PutProject(int id, UpdateProjectDto dto)
        {
            ActionResult<DetailedProjectDTO> result = NotFound();

            ActionResult<DetailedProjectDTO> project = await _projectService.GetDetailedProject(id);
            if (project != null)
            {
                result = Ok(project);
            }
            return result;
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public async Task<ActionResult<Project>> PostProject([FromBody] JsonElement json)
        {
            ActionResult result;

            string? name = json.TryGetProperty("name", out var nameProperty) ? nameProperty.GetString() : null;
            string? description = json.TryGetProperty("description", out var descriptionProperty) ? descriptionProperty.GetString() : null;

            if (name == null || description == null)
            {
                result = BadRequest("Name and description are required.");
            }
            else
            {
                int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                Project project =await  _projectService.CreateProject(name,description, teacherId);
                result = Ok(project);
            }

            return result;
        }

        [Authorize(Roles = "Teacher")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            ActionResult result = NotFound();
            

            if (_projectService.ProjectExists(id))
            {
                int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if (_projectService.ProjectBelongsToTeacher(id, teacherId))
                {
                    ActionResult<Project> project = await _projectService.DeleteProject(id);
                    result = Ok(project);
                }
                else
                {
                    result = Forbid("Teacher not in charge of project");
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
            else if (_projectService.ProjectBelongsToTeacher(projectId, teacherId) == false)
            {
                result = Forbid("Teacher not in charge of project");
            } 
            else if (_userService.UserExists(dto.StudentId) == false)
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
            ActionResult result ;
            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (_projectService.ProjectExists(projectId) == false)
            {
                result = NotFound("Project not found");
            }
            else if (_userService.UserExists(studentId) == false)
            {
                result = NotFound("Student not found");
            }
            else if (!_projectService.ProjectBelongsToTeacher(projectId, teacherId))
            {
                result = Forbid("Teacher not in charge of project");
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
