using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagementAPI.Data;
using ProjectManagementAPI.DTOs;
using ProjectManagementAPI.Models;

namespace ProjectManagementAPI.Services
{
    public class ProjectService
    {
        private readonly ApplicationContext _context;
        private readonly UserService _userService;

        public ProjectService(ApplicationContext context, UserService userService)
        {
            _context = context;
            _userService = userService; 
        }

        public async Task<ActionResult<IEnumerable<Project>>> GetAllProjects()
        {
            return await _context.Projects.ToListAsync();
        }

        public async Task<ActionResult<DetailedProjectDTO>> GetDetailedProject(int id)
        {
            ActionResult<DetailedProjectDTO> result = null;
            var project = await _context.Projects.FindAsync(id);

            if(project != null) 
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

                result = new DetailedProjectDTO()
                {
                    id = project.Id,
                    name = project.Name,
                    description = project.Description,
                    date = project.Date,
                    teacherId = project.TeacherId,
                    students = students

                };
            }
            return result;
        }

        public async Task<Project> CreateProject(string name, string description, int teacherId)
        {
            Project project = new Project()
            {
                Name = name,
                Description = description,
                TeacherId = teacherId,
                Date = DateTime.Now
            };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<ActionResult<Project>> DeleteProject(int projectId)
        {
            Project project = GetProjectById(projectId).Result;
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<StudentProject> AddStudentToProject(int projectId, int studentId, string role)
        {
            StudentProject relation = new StudentProject()
            {
                ProjectId = projectId,
                StudentId = studentId,
                Role = role
            };
            _context.StudentProjects.Add(relation);
            await _context.SaveChangesAsync();
            return relation;
        }

        public async Task<ActionResult<StudentProject>> DeleteStudentFromProject(int projectId, int studentId)
        {
            StudentProject relation = _context.StudentProjects.FirstOrDefault(sp => sp.ProjectId == projectId && sp.StudentId == studentId);
            _context.StudentProjects.Remove(relation);
            await _context.SaveChangesAsync();
            return relation;
        }

        public bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }

        public bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.Id == id);
        }

        public bool ProjectBelongsToTeacher(int projectId, int teacherId)
        {
            return _context.Projects.Any(p => p.Id == projectId && p.TeacherId == teacherId);
        }

        public async Task<Project> GetProjectById(int id)
        {
            return await _context.Projects.FindAsync(id);
        }

        public bool StudentBelongsToProject(int projectId, int studentId)
        {
            return _context.StudentProjects.Any(sp => sp.ProjectId == projectId && sp.StudentId == studentId);
        }
    }
}
