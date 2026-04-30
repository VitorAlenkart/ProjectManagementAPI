using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using ProjectManagementAPI.Data;
using ProjectManagementAPI.Models;


namespace ProjectManagementAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class SignUpController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public SignUpController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpPost("teacher")]
        public async Task<ActionResult<Teacher>> PostTeacher(Teacher teacher)
        {
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTeacher", new { id = teacher.id }, teacher);
        }

        // GET: api/Teachers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Teacher>>> GetTeacher()
        {
            return await _context.Teachers.ToListAsync();
        }

        [HttpPost("student")]
        public async Task<ActionResult<Student>> PostStudent(Student student)
        {
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
