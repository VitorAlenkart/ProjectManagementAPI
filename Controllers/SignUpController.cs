using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using ProjectManagementAPI.Data;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Services;


namespace ProjectManagementAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SignUpController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly PasswordService _passwordService;

        public SignUpController(ApplicationContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        [HttpPost("teacher")]
        public async Task<ActionResult<Teacher>> PostTeacher(Teacher teacher)
        {
            teacher.hashedPassword = _passwordService.HashPassword(teacher.hashedPassword);
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTeacher", new { id = teacher.id }, teacher);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Teacher>>> GetTeacher()
        {
            return await _context.Teachers.ToListAsync();
        }

        [HttpPost("student")]
        public async Task<ActionResult<Student>> PostStudent(Student student)
        {
            student.hashedPassword = _passwordService.HashPassword(student.hashedPassword);
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStudent", new { id = student.id }, student);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudent()
        {
            return await _context.Students.ToListAsync();
        }

    }
}
