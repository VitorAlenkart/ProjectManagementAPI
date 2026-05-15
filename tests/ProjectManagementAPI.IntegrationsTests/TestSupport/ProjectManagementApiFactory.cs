using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectManagementAPI.Data;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Services;

namespace ProjectManagementAPI.IntegrationsTests.TestSupport;

public sealed class ProjectManagementApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "this-is-a-test-secret-key-with-32-chars",
                ["Jwt:Issuer"] = "ProjectManagementAPI.Tests",
                ["Jwt:Audience"] = "ProjectManagementAPI.Tests",
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\mssqllocaldb;Database=ProjectManagementAPI.Tests;Trusted_Connection=True;"
            };

            configuration.AddInMemoryCollection(settings);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationContext>>();

            services.AddDbContext<ApplicationContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
            });
        });
    }

    public void ResetDatabase()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        Seed(context);
    }

    private static void Seed(ApplicationContext context)
    {
        var passwordService = new PasswordService();

        context.Teachers.AddRange(
            new Teacher
            {
                Id = 1,
                FullName = "Teacher Owner",
                Email = "teacher@example.com",
                HashedPassword = passwordService.HashPassword("Senha@123"),
                OccupationArea = "Software Engineering",
                FormationArea = "Computer Science"
            },
            new Teacher
            {
                Id = 2,
                FullName = "Teacher Other",
                Email = "other.teacher@example.com",
                HashedPassword = passwordService.HashPassword("Senha@123"),
                OccupationArea = "Data Science",
                FormationArea = "Mathematics"
            });

        context.Students.AddRange(
            new Student
            {
                Id = 10,
                FullName = "Student Linked",
                Email = "student@example.com",
                HashedPassword = passwordService.HashPassword("Senha@123"),
                EducationalInstitution = "Test University",
                Role = "Backend"
            },
            new Student
            {
                Id = 11,
                FullName = "Student Available",
                Email = "student.available@example.com",
                HashedPassword = passwordService.HashPassword("Senha@123"),
                EducationalInstitution = "Test University",
                Role = "Frontend"
            });

        context.Projects.AddRange(
            new Project
            {
                Id = 100,
                Name = "Seeded Project",
                Description = "Seeded Description",
                Date = new DateTime(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc),
                TeacherId = 1
            },
            new Project
            {
                Id = 200,
                Name = "Other Teacher Project",
                Description = "Other Description",
                Date = new DateTime(2026, 5, 15, 13, 0, 0, DateTimeKind.Utc),
                TeacherId = 2
            });

        context.StudentProjects.Add(new StudentProject
        {
            ProjectId = 100,
            StudentId = 10,
            Role = "Backend"
        });

        context.SaveChanges();
    }
}
