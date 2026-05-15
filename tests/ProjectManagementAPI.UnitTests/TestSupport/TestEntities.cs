using ProjectManagementAPI.Models;

namespace ProjectManagementAPI.UnitTests.TestSupport;

internal static class TestEntities
{
    public static Teacher Teacher(int id = 1)
    {
        Teacher result = new Teacher
        {
            Id = id,
            FullName = $"Teacher {id}",
            Email = $"teacher{id}@example.com",
            HashedPassword = "hashed-password",
            OccupationArea = "Software Engineering",
            FormationArea = "Computer Science"
        };

        return result;
    }

    public static Student Student(int id = 1)
    {
        Student result = new Student
        {
            Id = id,
            FullName = $"Student {id}",
            Email = $"student{id}@example.com",
            HashedPassword = "hashed-password",
            EducationalInstitution = "Test University",
            Role = "Developer"
        };

        return result;
    }

    public static Project Project(int id = 1, int teacherId = 1)
    {
        Project result = new Project
        {
            Id = id,
            Name = $"Project {id}",
            Description = $"Description {id}",
            Date = new DateTime(2026, 5, 15, 12, 0, 0, DateTimeKind.Utc),
            TeacherId = teacherId
        };

        return result;
    }

    public static StudentProject StudentProject(int projectId = 1, int studentId = 1, string role = "Developer")
    {
        StudentProject result = new StudentProject
        {
            ProjectId = projectId,
            StudentId = studentId,
            Role = role
        };

        return result;
    }
}
