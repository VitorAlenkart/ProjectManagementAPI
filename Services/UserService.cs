using ProjectManagementAPI.Data;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.DTOs;

namespace ProjectManagementAPI.Services
{
    public class UserService
    {
        private readonly ApplicationContext _context;

        public UserService(ApplicationContext context)
        {
            _context = context;
        }

        public StudentDTO? GetStudentById(int id)
        {
            Student student = _context.Students.FirstOrDefault(s => s.Id == id);
            StudentDTO user = new StudentDTO()
            {
                FullName = student.FullName,
                Email = student.Email,
                EducationalInstitution = student.EducationalInstitution
            };
            return user;
        }

    }
}
