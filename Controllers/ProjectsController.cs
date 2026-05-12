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

        // GET: api/Projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DetailedProjectDTO>> GetProject(int id)
        {
            ActionResult<DetailedProjectDTO> project = NotFound();
            if (_projectService.ProjectExists(id))
            {
                var result = await (
                from p in _context.Projects
                join t in _context.Teachers
                on p.TeacherId equals t.Id
                join sp in _context.StudentProjects
                on p.Id equals sp.ProjectId
                join s in _context.Students
                on sp.StudentId equals s.Id
                where p.Id == id

                select new
                {
                    id = p.Id,
                    name = p.Name,
                    description = p.Description,
                    teacherId = p.TeacherId,
                    teacherFullName = t.FullName,
                    date = p.Date,

                    Student = new
                    {
                        s.Id,
                        s.FullName,
                        s.Email,
                        sp.Role
                    }
                }
                ).ToListAsync();

                project = new DetailedProjectDTO()
                {
                    id = result.First().id,
                    name = result.First().name,
                    description = result.First().description,
                    teacherId = result.First().teacherId,
                    date = result.First().date,
                    students = (List<StudentDTO>)result.Select(r => r.Student)
                };
            }


            return project;
        }

        [Authorize(Roles = "Teacher")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProject(
            int id,
            UpdateProjectDto dto)
        {
            var project = await _context.Projects.FindAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            project.Name = dto.Name;
            project.Description = dto.Description;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Project>> PostProject([FromBody] JsonElement json)
        {
            string? name = json.TryGetProperty("name", out var nameProperty) ? nameProperty.GetString() : null;
            string? description = json.TryGetProperty("description", out var descriptionProperty) ? descriptionProperty.GetString() : null;

            if (name == null || description == null)
            {
                return BadRequest("Name and description are required.");
            }

            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var teacher = await _context.Teachers.FindAsync(teacherId);

            Project project = new()
            {
                Name = name,
                Description = description,
                TeacherId = teacherId,
                Date = DateTime.Now
            };

            _context.Projects.Add(project);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost("link/{projectId}/students")]
        public async Task<ActionResult> AddStudentToProject(int projectId, StudentProjectDTO dto)
        {
            ActionResult result = null;

            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (_projectService.ProjectExists(projectId))
            {
                var project = await _context.Projects.FindAsync(projectId);

                if (_projectService.ProjectBelongsToTeacher(projectId, teacherId))
                {
                    var student = await _context.Students.FindAsync(dto.StudentId);
                    if (student != null)
                    {
                        var existingRelation = await _context.StudentProjects
                            .FirstOrDefaultAsync(sp =>
                            sp.ProjectId == projectId &&
                            sp.StudentId == dto.StudentId);
                        if (existingRelation == null)
                        {
                            var relation = new StudentProject
                            {
                                ProjectId = projectId,
                                StudentId = dto.StudentId,
                                Role = dto.Role
                            };

                            _context.StudentProjects.Add(relation);
                            await _context.SaveChangesAsync();

                            result = Ok();
                        }
                        else
                        {
                            result = BadRequest("Student already in project.");
                        }
                    }
                    else
                    {
                        result = NotFound("Student not found.");
                    }
                    
                }
                else
                {
                    result = Forbid();
                }

            }
            else
            {
                result = NotFound();
            }


            /*            var student = await _context.Students.FindAsync(dto.StudentId);


                        var existingRelation = await _context.StudentProjects
                            .FirstOrDefaultAsync(sp =>
                            sp.ProjectId == projectId && sp.StudentId == dto.StudentId);

                        if (existingRelation != null)
                        {
                            return BadRequest("Student already in project.");
                        }

                        var relation = new StudentProject
                        {
                            ProjectId = projectId,
                            StudentId = dto.StudentId,
                            Role = dto.Role
                        };

                        _context.StudentProjects.Add(relation);

                        await _context.SaveChangesAsync();*/

            return result;
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteStudentFromProject(int projectId, int studentId)
        {
            ActionResult result = null;
            int teacherId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

            if (project != null)
            {

                if (_projectService.ProjectBelongsToTeacher(projectId, teacherId))
                {
                    if (!_projectService.StudentBelongsToProject(projectId, studentId))
                    {
                        result = NotFound("Student not in project.");
                    }
                    else
                    {
                        _context.StudentProjects.RemoveRange(_context.StudentProjects.Where(sp => sp.ProjectId == projectId && sp.StudentId == studentId));
                        await _context.SaveChangesAsync();
                        StudentDTO student = _userService.GetStudentById(studentId);
                        result = Ok("Student " + student.FullName + " removed from project.");
                    }

                }
                else
                {
                    result = Forbid();
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
