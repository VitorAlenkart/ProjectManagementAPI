using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ProjectManagementAPI.Data;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Services;
using ProjectManagementAPI.UnitTests.TestSupport;

namespace ProjectManagementAPI.UnitTests.Services;

public sealed class UserServiceTests
{
    [Fact]
    public void CreateStudent_WhenDataIsValid_PersistsStudentWithHashedPassword()
    {
        using var context = TestApplicationContextFactory.Create();
        var passwordService = new PasswordService();
        var service = CreateService(context, passwordService);

        Student student = service.createStudent("Ada Lovelace", "ada@example.com", "Senha@123", "Test University");

        Assert.Equal("Ada Lovelace", student.FullName);
        Assert.Equal("ada@example.com", student.Email);
        Assert.True(passwordService.VerifyPassword("Senha@123", student.HashedPassword));
        Assert.Single(context.Students);
    }

    [Fact]
    public void CreateTeacher_WhenDataIsValid_PersistsTeacherWithHashedPassword()
    {
        using var context = TestApplicationContextFactory.Create();
        var passwordService = new PasswordService();
        var service = CreateService(context, passwordService);

        Teacher teacher = service.createTeacher(
            "Grace Hopper",
            "grace@example.com",
            "Senha@123",
            "Architecture",
            "Computer Science");

        Assert.Equal("Grace Hopper", teacher.FullName);
        Assert.Equal("Architecture", teacher.OccupationArea);
        Assert.True(passwordService.VerifyPassword("Senha@123", teacher.HashedPassword));
        Assert.Single(context.Teachers);
    }

    [Fact]
    public void CreateUser_WhenEducationalInstitutionIsProvided_CreatesStudent()
    {
        using var context = TestApplicationContextFactory.Create();
        var service = CreateService(context);

        User user = service.createUser("Linus", "linus@example.com", "Senha@123", educationalInstitution: "Test University");

        Assert.IsType<Student>(user);
        Assert.True(context.Students.Any(s => s.Email == "linus@example.com"));
    }

    [Fact]
    public void CreateUser_WhenTeacherFieldsAreProvided_CreatesTeacher()
    {
        using var context = TestApplicationContextFactory.Create();
        var service = CreateService(context);

        User user = service.createUser(
            "Barbara",
            "barbara@example.com",
            "Senha@123",
            occupationArea: "Research",
            formationArea: "Mathematics");

        Assert.IsType<Teacher>(user);
        Assert.True(context.Teachers.Any(t => t.Email == "barbara@example.com"));
    }

    [Fact]
    public void CreateUser_WhenUserTypeFieldsAreMissing_ThrowsArgumentException()
    {
        using var context = TestApplicationContextFactory.Create();
        var service = CreateService(context);

        Assert.Throws<ArgumentException>(() => service.createUser("Invalid", "invalid@example.com", "Senha@123"));
    }

    [Fact]
    public void VerifyEmailExists_WhenEmailExistsForStudentOrTeacher_ReturnsTrue()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Students.Add(TestEntities.Student(1));
        context.Teachers.Add(TestEntities.Teacher(2));
        context.SaveChanges();
        var service = CreateService(context);

        Assert.True(service.verifyEmailExists("student1@example.com"));
        Assert.True(service.verifyEmailExists("teacher2@example.com"));
    }

    [Fact]
    public void VerifyEmailExists_WhenEmailDoesNotExist_ReturnsFalse()
    {
        using var context = TestApplicationContextFactory.Create();
        var service = CreateService(context);

        Assert.False(service.verifyEmailExists("nobody@example.com"));
    }

    [Fact]
    public void Login_WhenStudentCredentialsAreValid_ReturnsStudent()
    {
        using var context = TestApplicationContextFactory.Create();
        var passwordService = new PasswordService();
        context.Students.Add(new Student
        {
            Id = 1,
            FullName = "Student 1",
            Email = "student@example.com",
            HashedPassword = passwordService.HashPassword("Senha@123"),
            EducationalInstitution = "Test University",
            Role = "Developer"
        });
        context.SaveChanges();
        var service = CreateService(context, passwordService);

        User? user = service.login("student@example.com", "Senha@123");

        Assert.IsType<Student>(user);
    }

    [Fact]
    public void Login_WhenTeacherCredentialsAreValid_ReturnsTeacher()
    {
        using var context = TestApplicationContextFactory.Create();
        var passwordService = new PasswordService();
        context.Teachers.Add(new Teacher
        {
            Id = 1,
            FullName = "Teacher 1",
            Email = "teacher@example.com",
            HashedPassword = passwordService.HashPassword("Senha@123"),
            OccupationArea = "Software Engineering",
            FormationArea = "Computer Science"
        });
        context.SaveChanges();
        var service = CreateService(context, passwordService);

        User? user = service.login("teacher@example.com", "Senha@123");

        Assert.IsType<Teacher>(user);
    }

    [Fact]
    public void Login_WhenCredentialsAreInvalid_ReturnsNull()
    {
        using var context = TestApplicationContextFactory.Create();
        var passwordService = new PasswordService();
        context.Students.Add(new Student
        {
            Id = 1,
            FullName = "Student 1",
            Email = "student@example.com",
            HashedPassword = passwordService.HashPassword("Senha@123"),
            EducationalInstitution = "Test University",
            Role = "Developer"
        });
        context.SaveChanges();
        var service = CreateService(context, passwordService);

        Assert.Null(service.login("student@example.com", "wrong-password"));
        Assert.Null(service.login("missing@example.com", "Senha@123"));
    }

    [Fact]
    public void GenerateJwtToken_WhenUserIsProvided_ContainsExpectedClaims()
    {
        using var context = TestApplicationContextFactory.Create();
        var service = CreateService(context);
        var user = TestEntities.Teacher(10);

        JwtSecurityToken token = service.GenerateJwtToken(user, "Teacher");

        Assert.Equal("ProjectManagementAPI.Tests", token.Issuer);
        Assert.Contains(token.Audiences, audience => audience == "ProjectManagementAPI.Tests");
        Assert.Contains(token.Claims, claim => claim.Type == ClaimTypes.NameIdentifier && claim.Value == "10");
        Assert.Contains(token.Claims, claim => claim.Type == ClaimTypes.Email && claim.Value == user.Email);
        Assert.Contains(token.Claims, claim => claim.Type == ClaimTypes.Role && claim.Value == "Teacher");
    }

    [Fact]
    public void UserExists_WhenStudentOrTeacherExists_ReturnsTrue()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Students.Add(TestEntities.Student(1));
        context.Teachers.Add(TestEntities.Teacher(2));
        context.SaveChanges();
        var service = CreateService(context);

        Assert.True(service.UserExists(1));
        Assert.True(service.UserExists(2));
    }

    [Fact]
    public void UserExists_WhenUserDoesNotExist_ReturnsFalse()
    {
        using var context = TestApplicationContextFactory.Create();
        var service = CreateService(context);

        Assert.False(service.UserExists(99));
    }

    [Fact]
    public void GetStudentById_WhenStudentExists_ReturnsStudent()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Students.Add(TestEntities.Student(1));
        context.SaveChanges();
        var service = CreateService(context);

        Student? student = service.GetStudentById(1);

        Assert.NotNull(student);
        Assert.Equal("Student 1", student.FullName);
    }

    [Fact]
    public void GetStudentById_WhenStudentDoesNotExist_ReturnsNull()
    {
        using var context = TestApplicationContextFactory.Create();
        var service = CreateService(context);

        Assert.Null(service.GetStudentById(99));
    }

    private static UserService CreateService(ApplicationContext context, PasswordService? passwordService = null)
    {
        UserService result = new UserService(
            context,
            passwordService ?? new PasswordService(),
            ControllerTestHelpers.CreateJwtConfiguration());

        return result;
    }
}
