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
        private readonly ApplicationContext _context;
        private readonly ProjectService _projectService;
        private readonly UserService _userService;

        public ProjectsController(ApplicationContext context, ProjectService projectService, UserService userService)
        {
            _context = context;
            _projectService = projectService;
            _userService = userService;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
        {
            return await _context.Projects.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DetailedProjectDTO>> GetProject(int id)
        {
            ActionResult<DetailedProjectDTO> result = NotFound();
            Project project = await _context.Projects.FindAsync(id);

            if (project != null)
            {
                List<StudentProject> relations = _context.StudentProjects.Where(sp => sp.ProjectId == id).ToList();

                var students = new List<StudentDTO>();
                foreach (var rel in relations)
                {
                    var s = _userService.GetStudentById(rel.StudentId);
                    if (s != null)
                    {
                        students.Add(new StudentDTO
                        {
                            Id = s.Id,
                            FullName = s.FullName,
                            Email = s.Email,
                            EducationalInstitution = s.EducationalInstitution,
                            Role = rel.Role 
                        });
                    }
                }

                DetailedProjectDTO detailedProject = new()
                {
                    id = project.Id,
                    name = project.Name,
                    description = project.Description,
                    date = project.Date,
                    teacherId = project.TeacherId,
                    students = students

                };

                result = Ok(detailedProject);
            }


            return result;
        }

        [Authorize(Roles = "Teacher")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(int id, UpdateProjectDto dto)
        {
            ActionResult result = NotFound();
            var project = await _context.Projects.FindAsync(id);

            if (project != null)
            {
                int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (teacherId == project.TeacherId)
                {
                    if (dto.Name == null || dto.Description == null)
                    {
                        result = BadRequest("Name and description are required.");
                    }
                    else
                    {
                        project.Name = dto.Name;
                        project.Description = dto.Description;
                        result = Ok(project);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    result = Forbid("Teacher not owner of project");
                }
                
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

                Project project = new()
                {
                    Name = name,
                    Description = description,
                    TeacherId = teacherId,
                    Date = DateTime.Now
                };

                _context.Projects.Add(project);

                await _context.SaveChangesAsync();
                result = Ok(project);
            }

            return result;
        }

        [Authorize(Roles = "Teacher")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            ActionResult result = NotFound();
            var project = await _context.Projects.FindAsync(id);

            if (project != null)
            {
                int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                if (_projectService.ProjectBelongsToTeacher(id, teacherId))
                {
                    _context.Projects.Remove(project);
                    await _context.SaveChangesAsync();
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
            var project = await _context.Projects.FindAsync(projectId);
            var student = await _context.Students.FindAsync(dto.StudentId);

            if (project == null)
            {
                result = NotFound("Project not found");
            }
            else if (teacherId != project.TeacherId)
            {
                result = Forbid("Teacher not in charge of project");
            } 
            else if (student == null)
            {
                result = NotFound("Student not found");
            }
            else if (_projectService.StudentBelongsToProject(project.Id, student.Id))
            {
                result = BadRequest("Student already in Project");
            }
            else
            {
                var relation = new StudentProject
                {
                    ProjectId = projectId,
                    StudentId = dto.StudentId,
                    Role = dto.Role
                };

                _context.StudentProjects.Add(relation);
                await _context.SaveChangesAsync();

                result = Ok(relation);
            }
            
            return result;
        }

        [Authorize(Roles = "Teacher")]
        [HttpDelete("{projectId}/unlink/{studentId}")]
        public async Task<ActionResult> DeleteStudentFromProject(int projectId, int studentId)
        {
            ActionResult result;
            
            Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

            if (project != null)
            {
                int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                if (_projectService.ProjectBelongsToTeacher(projectId, teacherId))
                {
                    if (!_projectService.StudentBelongsToProject(projectId, studentId))
                    {
                        result = NotFound("Student not in project.");
                    }
                    else
                    {
                        StudentProject tupla = _context.StudentProjects.Where(sp => sp.ProjectId == projectId && sp.StudentId == studentId).FirstOrDefault();
                        _context.StudentProjects.RemoveRange(tupla);
                        await _context.SaveChangesAsync();
                        StudentDTO student = _userService.GetStudentById(studentId);
                        result = Ok("Student " + student.FullName + " removed from project.");
                    }

                }
                else
                {   
                    result = Forbid("");
                }
            }
            else
            {
                result = NotFound();
            }

            return result;



        }

    }
}
