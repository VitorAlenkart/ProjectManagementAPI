using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementAPI.Data;
using ProjectManagementAPI.DTOs;
using ProjectManagementAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace ProjectManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public ProjectsController(ApplicationContext context)
        {
            _context = context;
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
            if (ProjectExists(id))
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

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public async Task<ActionResult<Project>> PostProject(Project dto)
        {
            var teacher = await _context.Teachers.FindAsync(dto.TeacherId);

            if (teacher == null)
            {
                return BadRequest("Teacher not found.");
            }

            Project project = new()
            {
                Name = dto.Name,
                Description = dto.Description,
                TeacherId = dto.TeacherId,
                Date = DateTime.Now
            };

            _context.Projects.Add(project);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
        }

        [Authorize(Roles = "Teacher")]
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
        public async Task<ActionResult> AddStudentToProject(
            int projectId,
            StudentProjectDTO dto)
        {
            var project = await _context.Projects.FindAsync(projectId);

            if (project == null)
                return NotFound("Project not found.");

            var student = await _context.Students.FindAsync(dto.StudentId);

            if (student == null)
                return NotFound("Student not found.");
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

            await _context.SaveChangesAsync();

            return Ok();
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }

    }
}
