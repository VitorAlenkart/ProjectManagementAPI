using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using NuGet.DependencyResolver;
using ProjectManagementAPI.Data;
using ProjectManagementAPI.DTOs;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;


namespace ProjectManagementAPI.Controllers
{

    [Route("api/")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly PasswordService _passwordService;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationContext context, PasswordService passwordService, IConfiguration configuration)
        {
            _context = context;
            _passwordService = passwordService;
            _configuration = configuration;
        }

        [HttpPost("signup")]
        public async Task<ActionResult> Register([FromBody] JsonElement json)
        {
            String? email = json.TryGetProperty("email", out var emailProperty) ? emailProperty.GetString() : null;
            String? fullName = json.TryGetProperty("fullName", out var fullNameProperty) ? fullNameProperty.GetString() : null;
            String? occupationArea = json.TryGetProperty("occupationArea", out var occupationAreaProperty) ? occupationAreaProperty.GetString() : null;
            String? formationArea = json.TryGetProperty("formationArea", out var formationAreaProperty) ? formationAreaProperty.GetString() : null;
            String? educationalInstitution = json.TryGetProperty("educationalInstitution", out var educationalInstitutionProperty) ? educationalInstitutionProperty.GetString() : null;
            String? hashedPassword = json.TryGetProperty("password", out var passwordProperty) ? (_passwordService.HashPassword(passwordProperty.GetString()!)) : null;

            Student? existingStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.Email == email);

            Teacher? existingTeacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Email == email);

            ActionResult result;
            if (existingStudent == null && existingTeacher == null)
            {
                if (fullName != null && hashedPassword != null && email != null)
                {
                    if (educationalInstitution != null)
                    {
                        
                        result = await RegisterStudent(fullName,email,hashedPassword,educationalInstitution);

                    }
                    else if (occupationArea != null && formationArea != null)
                    {
                        
                        result = await RegisterTeacher(fullName, email, hashedPassword, occupationArea, formationArea);

                    }
                    else
                    {
                        result = BadRequest("Data necessary to register teacher or student is missing.");
                    }

                }
                else
                {
                    result = BadRequest("Invalid registration data.");
                }
            }
            else
            {
                result = BadRequest("Email already in use.");
            }

            return result;
            
        }

        public async Task<ActionResult> RegisterStudent(String fullName, String email, String hashedPassword, String educationalInstitution)
        {
            Student student = new()
            {
                FullName = fullName,
                Email = email!,
                HashedPassword = hashedPassword,
                EducationalInstitution = educationalInstitution
            };

            _context.Students.Add(student);

            await _context.SaveChangesAsync();

            return Created();
        }

        public async Task<ActionResult> RegisterTeacher(String fullName, String email, String hashedPassword, String ocupationArea, String formationArea)
        {

            Teacher teacher = new()
            {
                FullName = fullName,
                Email = email,
                HashedPassword = hashedPassword,
                OccupationArea = ocupationArea,
                FormationArea = formationArea

            };

            _context.Teachers.Add(teacher);

            await _context.SaveChangesAsync();

            return Created();
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDto dto)
        {
            ActionResult result = null;

            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.Email == dto.Email);

            var teacher = await _context.Teachers
                    .FirstOrDefaultAsync(t => t.Email == dto.Email);

            User? user = null;
            string role = "";

            if (student != null)
            {
                bool validPassword = _passwordService.VerifyPassword(dto.Password, student.HashedPassword);

                if (validPassword)
                {
                    role = "Student";
                    user = student;
                }
                else
                    result = Unauthorized();
            }
            else if(teacher != null)
            {
                bool validPassword = _passwordService.VerifyPassword(dto.Password, teacher.HashedPassword);

                if (validPassword) 
                { 
                    role = "Teacher";
                    user = teacher;
                }
                else
                    return Unauthorized();
            }

            if (user != null)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, dto.Email),
                    new Claim(ClaimTypes.Role, role)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

                var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddHours(2),
                    signingCredentials: creds
                );

                result = Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }
            else
            {
                result = Unauthorized();
            }

            return result;
        }
    }
}
