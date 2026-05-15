using Humanizer;
using Microsoft.IdentityModel.Tokens;
using ProjectManagementAPI.Data;
using ProjectManagementAPI.DTOs;
using ProjectManagementAPI.Models;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProjectManagementAPI.Services
{
    public class UserService
    {
        private readonly ApplicationContext _context;
        private readonly PasswordService _passwordService;
        private readonly IConfiguration _configuration;

        public UserService(ApplicationContext context, PasswordService passwordService, IConfiguration configuration)
        {
            _context = context;
            _passwordService = passwordService;
            _configuration = configuration;
        }

        public Student createStudent(string fullName, string email, string password, string educationalInstitution)
        {
            Student result = new Student()
            {
                FullName = fullName,
                Email = email,
                HashedPassword = _passwordService.HashPassword(password),
                EducationalInstitution = educationalInstitution
            };

            _context.Students.Add(result);
            _context.SaveChanges();

            return result;
        }

        public Teacher createTeacher(string fullName, string email, string password, string occupationArea, string formationArea)
        {
            Teacher result = new Teacher()
            {
                FullName = fullName,
                Email = email,
                HashedPassword = _passwordService.HashPassword(password),
                OccupationArea = occupationArea,
                FormationArea = formationArea
            };

            _context.Teachers.Add(result);
            _context.SaveChanges();

            return result;
        }

        public User createUser(string fullName, string email, string password, string? educationalInstitution = null, string? occupationArea = null, string? formationArea = null)
        {
            User result;

            if (educationalInstitution != null)
            {
                result = createStudent(fullName, email, password, educationalInstitution);
            }
            else if (occupationArea != null && formationArea != null)
            {
                result = createTeacher(fullName, email, password, occupationArea, formationArea);
            }
            else
            {
                throw new ArgumentException("Invalid user type");
            }

            return result;
        }

        public bool verifyEmailExists(string email)
        {
            bool result = _context.Students.Any(s => s.Email == email) || _context.Teachers.Any(t => t.Email == email);

            return result;
        }

        public User? login(string email, string password)
        {
            User? user = null;

            var student = _context.Students.FirstOrDefault(s => s.Email == email);
            var teacher = _context.Teachers.FirstOrDefault(t => t.Email == email);

            if (student != null)
            {
                if (_passwordService.VerifyPassword(password, student.HashedPassword))
                {
                    user = student;
                }
            }
            else if (teacher != null)
            {
                if (_passwordService.VerifyPassword(password, teacher.HashedPassword))
                {
                    user = teacher;
                }
            }

            return user;
        }

        public JwtSecurityToken GenerateJwtToken(User user, string role)
        {
            var claims = new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, role)
                    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );
            return token;
        }

        public bool UserExists(int id)
        {
            bool result = _context.Students.Any(s => s.Id == id) || _context.Teachers.Any(t => t.Id == id);

            return result;
        }

        public Student? GetStudentById(int id)
        {
            Student? result = _context.Students.FirstOrDefault(s => s.Id == id);

            return result;
        }
    }
}

