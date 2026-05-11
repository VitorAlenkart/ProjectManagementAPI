using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ProjectManagementAPI.DTOs;
using ProjectManagementAPI.Data;


namespace ProjectManagementAPI.Controllers
{

    [Route("api/")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly PasswordService _passwordService;
        private readonly IConfiguration _configuration;

        public AuthController(
            ApplicationContext context,
            PasswordService passwordService,
            IConfiguration configuration)
        {
            _context = context;
            _passwordService = passwordService;
            _configuration = configuration;
        }

        [HttpPost("signup/student")]
        public async Task<ActionResult> RegisterStudent(CreateStudentDto dto)
        {
            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.Email == dto.Email);

            var existingTeacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Email == dto.Email);

            if (existingStudent != null || existingTeacher != null)
            {
                return BadRequest("Email already exists.");
            }

            Student student = new()
            {
                FullName = dto.FullName,
                Email = dto.Email,
                HashedPassword = _passwordService.HashPassword(dto.Password),
                EducationalInstitution = dto.EducationalInstitution
            };

            _context.Students.Add(student);

            await _context.SaveChangesAsync();

            return Ok("Student created successfully.");
        }
        [HttpPost("signup/teacher")]
        public async Task<ActionResult> RegisterTeacher(CreateTeacherDto dto)
        {
            var existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.Email == dto.Email);

            var existingTeacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Email == dto.Email);

            if (existingStudent != null || existingTeacher != null)
            {
                return BadRequest("Email already exists.");
            }

            Teacher teacher = new()
            {
                FullName = dto.FullName,
                Email = dto.Email,
                HashedPassword = _passwordService.HashPassword(dto.Password),
                OccupationArea = dto.OccupationArea,
                FormationArea = dto.FormationArea
            };

            _context.Teachers.Add(teacher);

            await _context.SaveChangesAsync();

            return Ok("Teacher created successfully.");
        }
        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto dto)
        {
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Email == dto.Email);

            string role = "";

            if (student != null)
            {
                bool validPassword = _passwordService.VerifyPassword(
                    dto.Password,
                    student.HashedPassword
                );

                if (!validPassword)
                    return Unauthorized();
                role = "Student";
            }
            else
            {
                var teacher = await _context.Teachers
                    .FirstOrDefaultAsync(t => t.Email == dto.Email);

                if (teacher == null)
                    return Unauthorized();

                bool validPassword = _passwordService.VerifyPassword(
                    dto.Password,
                    teacher.HashedPassword
                );

                if (!validPassword)
                    return Unauthorized();
                role = "Teacher";
            }

            var claims = new[]
            {
        new Claim(ClaimTypes.Email, dto.Email),
        new Claim(ClaimTypes.Role, role)
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }
    }
}
