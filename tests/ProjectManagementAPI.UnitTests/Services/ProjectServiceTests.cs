using ProjectManagementAPI.Data;
using ProjectManagementAPI.DTOs;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Services;
using ProjectManagementAPI.UnitTests.TestSupport;

namespace ProjectManagementAPI.UnitTests.Services;

public sealed class ProjectServiceTests
{
    [Fact]
    public async Task GetAllProjects_WhenProjectsExist_ReturnsAllProjects()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Projects.AddRange(TestEntities.Project(1), TestEntities.Project(2));
        context.SaveChanges();
        var service = CreateService(context);

        List<Project> projects = await service.GetAllProjects();

        Assert.Equal(2, projects.Count);
    }

    [Fact]
    public async Task GetDetailedProject_WhenProjectExists_ReturnsProjectWithStudents()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Teachers.Add(TestEntities.Teacher(1));
        context.Students.Add(TestEntities.Student(2));
        context.Projects.Add(TestEntities.Project(10, teacherId: 1));
        context.StudentProjects.Add(TestEntities.StudentProject(projectId: 10, studentId: 2, role: "Backend"));
        context.SaveChanges();
        var service = CreateService(context);

        DetailedProjectDTO? project = await service.GetDetailedProject(10);

        Assert.NotNull(project);
        Assert.Equal("Project 10", project.name);
        Assert.Equal(1, project.teacherId);
        StudentDTO student = Assert.Single(project.students);
        Assert.Equal(2, student.Id);
        Assert.Equal("Backend", student.Role);
    }

    [Fact]
    public async Task GetDetailedProject_WhenProjectDoesNotExist_ReturnsNull()
    {
        using var context = TestApplicationContextFactory.Create();
        var service = CreateService(context);

        DetailedProjectDTO? project = await service.GetDetailedProject(99);

        Assert.Null(project);
    }

    [Fact]
    public async Task CreateProject_WhenDataIsValid_PersistsProject()
    {
        using var context = TestApplicationContextFactory.Create();
        var service = CreateService(context);

        Project project = await service.CreateProject("New Project", "New Description", teacherId: 1);

        Assert.Equal("New Project", project.Name);
        Assert.Equal(1, project.TeacherId);
        Assert.Single(context.Projects);
    }

    [Fact]
    public async Task UpdateProject_WhenProjectExists_UpdatesNameAndDescription()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Projects.Add(TestEntities.Project(1));
        context.SaveChanges();
        var service = CreateService(context);

        Project? project = await service.UpdateProject(1, "Updated", "Updated description");

        Assert.NotNull(project);
        Assert.Equal("Updated", project.Name);
        Assert.Equal("Updated description", context.Projects.Single().Description);
    }

    [Fact]
    public async Task UpdateProject_WhenProjectDoesNotExist_ReturnsNull()
    {
        using var context = TestApplicationContextFactory.Create();
        var service = CreateService(context);

        Project? project = await service.UpdateProject(99, "Updated", "Updated description");

        Assert.Null(project);
    }

    [Fact]
    public async Task DeleteProject_WhenProjectExists_RemovesProject()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Projects.Add(TestEntities.Project(1));
        context.SaveChanges();
        var service = CreateService(context);

        Project? project = await service.DeleteProject(1);

        Assert.NotNull(project);
        Assert.Empty(context.Projects);
    }

    [Fact]
    public async Task DeleteProject_WhenProjectDoesNotExist_ReturnsNull()
    {
        using var context = TestApplicationContextFactory.Create();
        var service = CreateService(context);

        Project? project = await service.DeleteProject(99);

        Assert.Null(project);
    }

    [Fact]
    public async Task AddStudentToProject_WhenDataIsValid_PersistsRelation()
    {
        using var context = TestApplicationContextFactory.Create();
        var service = CreateService(context);

        StudentProject relation = await service.AddStudentToProject(1, 2, "Frontend");

        Assert.Equal(1, relation.ProjectId);
        Assert.Equal(2, relation.StudentId);
        Assert.Equal("Frontend", relation.Role);
        Assert.Single(context.StudentProjects);
    }

    [Fact]
    public async Task DeleteStudentFromProject_WhenRelationExists_RemovesRelation()
    {
        using var context = TestApplicationContextFactory.Create();
        context.StudentProjects.Add(TestEntities.StudentProject(projectId: 1, studentId: 2));
        context.SaveChanges();
        var service = CreateService(context);

        StudentProject? relation = await service.DeleteStudentFromProject(1, 2);

        Assert.NotNull(relation);
        Assert.Empty(context.StudentProjects);
    }

    [Fact]
    public async Task DeleteStudentFromProject_WhenRelationDoesNotExist_ReturnsNull()
    {
        using var context = TestApplicationContextFactory.Create();
        var service = CreateService(context);

        StudentProject? relation = await service.DeleteStudentFromProject(1, 2);

        Assert.Null(relation);
    }

    [Fact]
    public void ProjectExists_WhenProjectExists_ReturnsTrue()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Projects.Add(TestEntities.Project(1));
        context.SaveChanges();
        var service = CreateService(context);

        Assert.True(service.ProjectExists(1));
    }

    [Fact]
    public void ProjectExists_WhenProjectDoesNotExist_ReturnsFalse()
    {
        using var context = TestApplicationContextFactory.Create();
        var service = CreateService(context);

        Assert.False(service.ProjectExists(99));
    }

    [Fact]
    public void TeacherExists_WhenTeacherExists_ReturnsTrue()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Teachers.Add(TestEntities.Teacher(1));
        context.SaveChanges();
        var service = CreateService(context);

        Assert.True(service.TeacherExists(1));
    }

    [Fact]
    public void ProjectBelongsToTeacher_WhenTeacherOwnsProject_ReturnsTrue()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Projects.Add(TestEntities.Project(1, teacherId: 7));
        context.SaveChanges();
        var service = CreateService(context);

        Assert.True(service.ProjectBelongsToTeacher(1, 7));
        Assert.False(service.ProjectBelongsToTeacher(1, 8));
    }

    [Fact]
    public async Task GetProjectById_WhenProjectExists_ReturnsProject()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Projects.Add(TestEntities.Project(1));
        context.SaveChanges();
        var service = CreateService(context);

        Project? project = await service.GetProjectById(1);

        Assert.NotNull(project);
        Assert.Equal("Project 1", project.Name);
    }

    [Fact]
    public void StudentBelongsToProject_WhenRelationExists_ReturnsTrue()
    {
        using var context = TestApplicationContextFactory.Create();
        context.StudentProjects.Add(TestEntities.StudentProject(projectId: 1, studentId: 2));
        context.SaveChanges();
        var service = CreateService(context);

        Assert.True(service.StudentBelongsToProject(1, 2));
        Assert.False(service.StudentBelongsToProject(1, 3));
    }

    private static ProjectService CreateService(ApplicationContext context)
    {
        var userService = new UserService(
            context,
            new PasswordService(),
            ControllerTestHelpers.CreateJwtConfiguration());

        ProjectService result = new ProjectService(context, userService);

        return result;
    }
}
