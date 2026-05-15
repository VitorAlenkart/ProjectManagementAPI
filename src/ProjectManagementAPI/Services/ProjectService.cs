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

        public async Task<List<Project>> GetAllProjects()
        {
            List<Project> result = await _context.Projects.ToListAsync();

            return result;
        }

        public async Task<DetailedProjectDTO?> GetDetailedProject(int id)
        {
            DetailedProjectDTO? result = null;
            var project = await _context.Projects.FindAsync(id);

            if (project != null)
            {
                var relations = await _context.StudentProjects
                    .Where(sp => sp.ProjectId == id)
                    .ToListAsync();

                var students = new List<StudentDTO>();

                foreach (var relation in relations)
                {
                    var student = _userService.GetStudentById(relation.StudentId);

                    if (student != null)
                    {
                        students.Add(new StudentDTO
                        {
                            Id = student.Id,
                            FullName = student.FullName,
                            Email = student.Email,
                            EducationalInstitution = student.EducationalInstitution,
                            Role = relation.Role
                        });
                    }
                }

                result = new DetailedProjectDTO
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
            Project result = new()
            {
                Name = name,
                Description = description,
                TeacherId = teacherId,
                Date = DateTime.Now
            };

            _context.Projects.Add(result);
            await _context.SaveChangesAsync();

            return result;
        }

        public async Task<Project?> UpdateProject(int projectId, string name, string description)
        {
            Project? result = await GetProjectById(projectId);

            if (result != null)
            {
                result.Name = name;
                result.Description = description;

                await _context.SaveChangesAsync();
            }

            return result;
        }

        public async Task<Project?> DeleteProject(int projectId)
        {
            Project? result = await GetProjectById(projectId);

            if (result != null)
            {
                _context.Projects.Remove(result);
                await _context.SaveChangesAsync();
            }

            return result;
        }

        public async Task<StudentProject> AddStudentToProject(int projectId, int studentId, string role)
        {
            StudentProject result = new()
            {
                ProjectId = projectId,
                StudentId = studentId,
                Role = role
            };

            _context.StudentProjects.Add(result);
            await _context.SaveChangesAsync();

            return result;
        }

        public async Task<StudentProject?> DeleteStudentFromProject(int projectId, int studentId)
        {
            StudentProject? result = await _context.StudentProjects
                .FirstOrDefaultAsync(sp => sp.ProjectId == projectId && sp.StudentId == studentId);

            if (result != null)
            {
                _context.StudentProjects.Remove(result);
                await _context.SaveChangesAsync();
            }

            return result;
        }

        public bool ProjectExists(int id)
        {
            bool result = _context.Projects.Any(e => e.Id == id);

            return result;
        }

        public bool TeacherExists(int id)
        {
            bool result = _context.Teachers.Any(e => e.Id == id);

            return result;
        }

        public bool ProjectBelongsToTeacher(int projectId, int teacherId)
        {
            bool result = _context.Projects.Any(p => p.Id == projectId && p.TeacherId == teacherId);

            return result;
        }

        public async Task<Project?> GetProjectById(int id)
        {
            Project? result = await _context.Projects.FindAsync(id);

            return result;
        }

        public bool StudentBelongsToProject(int projectId, int studentId)
        {
            bool result = _context.StudentProjects.Any(sp => sp.ProjectId == projectId && sp.StudentId == studentId);

            return result;
        }
    }
}
