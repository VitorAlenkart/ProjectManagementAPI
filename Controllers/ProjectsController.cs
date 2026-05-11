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
            return await _context.Projects
                .Include(p => p.StudentProjects)
                .ThenInclude(sp => sp.Student)
                .ToListAsync();
        }

        // GET: api/Projects/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetProject(int id)
        {
            var project = await _context.Projects
                .Include(p => p.StudentProjects)
                .ThenInclude(sp => sp.Student)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
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
        public async Task<ActionResult<Project>> PostProject(CreateProjectDto dto)
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
        [HttpPost("{projectId}/students")]
        public async Task<ActionResult> AddStudentToProject(
            int projectId,
            AddStudentToProjectDto dto)
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
