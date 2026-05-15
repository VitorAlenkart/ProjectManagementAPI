using Microsoft.AspNetCore.Mvc;
using ProjectManagementAPI.Controllers;
using ProjectManagementAPI.Data;
using ProjectManagementAPI.DTOs;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Services;
using ProjectManagementAPI.UnitTests.TestSupport;

namespace ProjectManagementAPI.UnitTests.Controllers;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task Register_WhenStudentPayloadIsValid_ReturnsOkAndCreatesStudent()
    {
        using var context = TestApplicationContextFactory.Create();
        var passwordService = new PasswordService();
        var controller = CreateController(context, passwordService);
        var payload = ControllerTestHelpers.ToJsonElement(new
        {
            email = "student@example.com",
            fullName = "Student Example",
            password = "Senha@123",
            educationalInstitution = "Test University"
        });

        ActionResult result = await controller.Register(payload);

        var ok = Assert.IsType<OkObjectResult>(result);
        var student = Assert.IsType<Student>(ok.Value);
        Assert.Equal("student@example.com", student.Email);
        Assert.True(passwordService.VerifyPassword("Senha@123", student.HashedPassword));
        Assert.Single(context.Students);
    }

    [Fact]
    public async Task Register_WhenTeacherPayloadIsValid_ReturnsOkAndCreatesTeacher()
    {
        using var context = TestApplicationContextFactory.Create();
        var passwordService = new PasswordService();
        var controller = CreateController(context, passwordService);
        var payload = ControllerTestHelpers.ToJsonElement(new
        {
            email = "teacher@example.com",
            fullName = "Teacher Example",
            password = "Senha@123",
            occupationArea = "Software Engineering",
            formationArea = "Computer Science"
        });

        ActionResult result = await controller.Register(payload);

        var ok = Assert.IsType<OkObjectResult>(result);
        var teacher = Assert.IsType<Teacher>(ok.Value);
        Assert.Equal("teacher@example.com", teacher.Email);
        Assert.True(passwordService.VerifyPassword("Senha@123", teacher.HashedPassword));
        Assert.Single(context.Teachers);
    }

    [Fact]
    public async Task Register_WhenEmailAlreadyExists_ReturnsBadRequest()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Students.Add(TestEntities.Student(1));
        context.SaveChanges();
        var controller = CreateController(context);
        var payload = ControllerTestHelpers.ToJsonElement(new
        {
            email = "student1@example.com",
            fullName = "Student Example",
            password = "Senha@123",
            educationalInstitution = "Test University"
        });

        ActionResult result = await controller.Register(payload);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Email already in use.", badRequest.Value);
    }

    [Fact]
    public async Task Register_WhenRequiredDataIsMissing_ReturnsBadRequest()
    {
        using var context = TestApplicationContextFactory.Create();
        var controller = CreateController(context);
        var payload = ControllerTestHelpers.ToJsonElement(new
        {
            email = "student@example.com",
            password = "Senha@123"
        });

        ActionResult result = await controller.Register(payload);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid registration data.", badRequest.Value);
    }

    [Fact]
    public async Task Login_WhenStudentCredentialsAreValid_ReturnsToken()
    {
        using var context = TestApplicationContextFactory.Create();
        var passwordService = new PasswordService();
        context.Students.Add(new Student
        {
            Id = 1,
            FullName = "Student Example",
            Email = "student@example.com",
            HashedPassword = passwordService.HashPassword("Senha@123"),
            EducationalInstitution = "Test University",
            Role = "Developer"
        });
        context.SaveChanges();
        var controller = CreateController(context, passwordService);

        ActionResult result = await controller.Login(new LoginDto
        {
            Email = "student@example.com",
            Password = "Senha@123"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.False(string.IsNullOrWhiteSpace(ReadToken(ok.Value)));
    }

    [Fact]
    public async Task Login_WhenTeacherCredentialsAreValid_ReturnsToken()
    {
        using var context = TestApplicationContextFactory.Create();
        var passwordService = new PasswordService();
        context.Teachers.Add(new Teacher
        {
            Id = 1,
            FullName = "Teacher Example",
            Email = "teacher@example.com",
            HashedPassword = passwordService.HashPassword("Senha@123"),
            OccupationArea = "Software Engineering",
            FormationArea = "Computer Science"
        });
        context.SaveChanges();
        var controller = CreateController(context, passwordService);

        ActionResult result = await controller.Login(new LoginDto
        {
            Email = "teacher@example.com",
            Password = "Senha@123"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.False(string.IsNullOrWhiteSpace(ReadToken(ok.Value)));
    }

    [Fact]
    public async Task Login_WhenCredentialsAreInvalid_ReturnsUnauthorized()
    {
        using var context = TestApplicationContextFactory.Create();
        var passwordService = new PasswordService();
        context.Students.Add(new Student
        {
            Id = 1,
            FullName = "Student Example",
            Email = "student@example.com",
            HashedPassword = passwordService.HashPassword("Senha@123"),
            EducationalInstitution = "Test University",
            Role = "Developer"
        });
        context.SaveChanges();
        var controller = CreateController(context, passwordService);

        ActionResult result = await controller.Login(new LoginDto
        {
            Email = "student@example.com",
            Password = "wrong-password"
        });

        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Login_WhenEmailOrPasswordIsMissing_ReturnsBadRequest()
    {
        using var context = TestApplicationContextFactory.Create();
        var controller = CreateController(context);

        ActionResult result = await controller.Login(new LoginDto
        {
            Email = null!,
            Password = null!
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Email and password are required.", badRequest.Value);
    }

    private static AuthController CreateController(ApplicationContext context, PasswordService? passwordService = null)
    {
        var userService = new UserService(
            context,
            passwordService ?? new PasswordService(),
            ControllerTestHelpers.CreateJwtConfiguration());

        AuthController result = new AuthController(userService);

        return result;
    }

    private static string? ReadToken(object? value)
    {
        string? result = value?.GetType().GetProperty("token")?.GetValue(value) as string;

        return result;
    }
}
