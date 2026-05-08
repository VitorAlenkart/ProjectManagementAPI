using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.DependencyResolver;
using ProjectManagementAPI.Data;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Services;


namespace ProjectManagementAPI.Controllers
{

    [Route("api/")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationContext _context;
        private readonly PasswordService _passwordService;

        public AuthController(ApplicationContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        [HttpPost("signup")]
        public async Task<ActionResult> PostUser(JsonElement body)
        {
            body.TryGetProperty("hashedPassword", out JsonElement passwordElement);
            body.TryGetProperty("email", out JsonElement emailElement);
            body.TryGetProperty("fullName", out JsonElement fullNameElement);

            if(passwordElement.ValueKind == JsonValueKind.Undefined || emailElement.ValueKind == JsonValueKind.Undefined || fullNameElement.ValueKind == JsonValueKind.Undefined)
            {
                return BadRequest("Invalid input: missing required fields.");
            }
            else
            {
                if (body.TryGetProperty("educationalInstitution", out JsonElement educationalInstitution) && educationalInstitution.ValueKind != JsonValueKind.Undefined)
                {
                    Student student = new Student
                    {
                        fullName = fullNameElement.GetString()!,
                        email = emailElement.GetString()!,
                        hashedPassword = passwordElement.GetString()!,
                        educationalInstitution = educationalInstitution.GetString()!
                    };
                    await PostStudent(student);
                }
                else if(body.TryGetProperty("occupationArea", out JsonElement occupationArea) && occupationArea.ValueKind != JsonValueKind.Undefined && body.TryGetProperty("formationArea", out JsonElement formationArea) && formationArea.ValueKind != JsonValueKind.Undefined)
                {
                    Teacher teacher = new Teacher
                    {
                        fullName = fullNameElement.GetString()!,
                        email = emailElement.GetString()!,
                        hashedPassword = passwordElement.GetString()!,
                        occupationArea = occupationArea.GetString()!,
                        formationArea = formationArea.GetString()!
                    };
                    await PostTeacher(teacher);
                }
                else
                {
                    return BadRequest("Invalid input: missing required fields for either Student or Teacher.");
                }
                return Created();
            }   
        }

        public async Task PostTeacher(Teacher teacher)
        {
            teacher.hashedPassword = _passwordService.HashPassword(teacher.hashedPassword);
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync(); 
        }

        public async Task PostStudent(Student student)
        {
            student.hashedPassword = _passwordService.HashPassword(student.hashedPassword);
            _context.Students.Add(student);
            await _context.SaveChangesAsync();
        }

    }
}
