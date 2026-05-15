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
            Student student = new Student()
            {
                FullName = fullName,
                Email = email,
                HashedPassword = _passwordService.HashPassword(password),
                EducationalInstitution = educationalInstitution
            };
            _context.Students.Add(student);
            _context.SaveChanges();
            return student;
        }

        public Teacher createTeacher(string fullName, string email, string password, string occupationArea, string formationArea)
        {
            Teacher teacher = new Teacher()
            {
                FullName = fullName,
                Email = email,
                HashedPassword = _passwordService.HashPassword(password),
                OccupationArea = occupationArea,
                FormationArea = formationArea
            };
            _context.Teachers.Add(teacher);
            _context.SaveChanges();
            return teacher;
        }

        public User createUser(string fullName, string email, string password, string? educationalInstitution = null, string? occupationArea = null, string? formationArea = null)
        {
            if (educationalInstitution != null)
            {
                return createStudent(fullName, email, password, educationalInstitution);
            }
            else if (occupationArea != null && formationArea != null)
            {
                return createTeacher(fullName, email, password, occupationArea, formationArea);
            }
            else
            {
                throw new ArgumentException("Invalid user type");
            }
        }

        public bool verifyEmailExists(string email)
        {
            return _context.Students.Any(s => s.Email == email) || _context.Teachers.Any(t => t.Email == email);
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
            return _context.Students.Any(s => s.Id == id) || _context.Teachers.Any(t => t.Id == id);
        }

        public Student? GetStudentById(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.Id == id);
            return student;
        }
    }
}

