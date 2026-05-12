using Microsoft.AspNetCore.Identity;
using ProjectManagementAPI.Data;
using ProjectManagementAPI.Models;

namespace ProjectManagementAPI.Services
{
    public class ProjectService
    {
        private readonly ApplicationContext _context;

        public ProjectService(ApplicationContext context)
        {
            _context = context;
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

        public async Task<Project> getProjectById(int id)
        {
            return await _context.Projects.FindAsync(id);
        }
    }
}
