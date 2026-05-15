using Microsoft.AspNetCore.Mvc;
using ProjectManagementAPI.Controllers;
using ProjectManagementAPI.Data;
using ProjectManagementAPI.DTOs;
using ProjectManagementAPI.Models;
using ProjectManagementAPI.Services;
using ProjectManagementAPI.UnitTests.TestSupport;

namespace ProjectManagementAPI.UnitTests.Controllers;

public sealed class ProjectsControllerTests
{
    [Fact]
    public async Task GetProjects_WhenProjectsExist_ReturnsOkWithProjects()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Projects.AddRange(TestEntities.Project(1), TestEntities.Project(2));
        context.SaveChanges();
        var controller = CreateController(context);

        ActionResult<IEnumerable<Project>> result = await controller.GetProjects();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var projects = Assert.IsAssignableFrom<IEnumerable<Project>>(ok.Value);
        Assert.Equal(2, projects.Count());
    }

    [Fact]
    public async Task GetProject_WhenProjectDoesNotExist_ReturnsNotFound()
    {
        using var context = TestApplicationContextFactory.Create();
        var controller = CreateController(context);

        ActionResult<DetailedProjectDTO> result = await controller.GetProject(99);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetProject_WhenProjectExists_ReturnsOkWithDetailedProject()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Teachers.Add(TestEntities.Teacher(1));
        context.Students.Add(TestEntities.Student(2));
        context.Projects.Add(TestEntities.Project(10, teacherId: 1));
        context.StudentProjects.Add(TestEntities.StudentProject(projectId: 10, studentId: 2, role: "Backend"));
        context.SaveChanges();
        var controller = CreateController(context);

        ActionResult<DetailedProjectDTO> result = await controller.GetProject(10);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var project = Assert.IsType<DetailedProjectDTO>(ok.Value);
        Assert.Equal("Project 10", project.name);
        Assert.Equal("Backend", Assert.Single(project.students).Role);
    }

    [Fact]
    public async Task PutProject_WhenProjectDoesNotExist_ReturnsNotFound()
    {
        using var context = TestApplicationContextFactory.Create();
        var controller = CreateController(context);

        IActionResult result = await controller.PutProject(99, new UpdateProjectDto
        {
            Name = "Updated",
            Description = "Updated description"
        });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task PutProject_WhenProjectExists_UpdatesProjectAndReturnsNoContent()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Projects.Add(TestEntities.Project(1));
        context.SaveChanges();
        var controller = CreateController(context);

        IActionResult result = await controller.PutProject(1, new UpdateProjectDto
        {
            Name = "Updated",
            Description = "Updated description"
        });

        Assert.IsType<NoContentResult>(result);
        Project project = Assert.Single(context.Projects);
        Assert.Equal("Updated", project.Name);
        Assert.Equal("Updated description", project.Description);
    }

    [Fact]
    public async Task PostProject_WhenPayloadIsInvalid_ReturnsBadRequest()
    {
        using var context = TestApplicationContextFactory.Create();
        var controller = CreateController(context);
        ControllerTestHelpers.SetAuthenticatedUser(controller, userId: 1);
        var payload = ControllerTestHelpers.ToJsonElement(new
        {
            name = "Project without description"
        });

        ActionResult<Project> result = await controller.PostProject(payload);

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Name and description are required.", badRequest.Value);
    }

    [Fact]
    public async Task PostProject_WhenPayloadIsValid_CreatesProjectForAuthenticatedTeacher()
    {
        using var context = TestApplicationContextFactory.Create();
        var controller = CreateController(context);
        ControllerTestHelpers.SetAuthenticatedUser(controller, userId: 7);
        var payload = ControllerTestHelpers.ToJsonElement(new
        {
            name = "New Project",
            description = "New Description"
        });

        ActionResult<Project> result = await controller.PostProject(payload);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var project = Assert.IsType<Project>(ok.Value);
        Assert.Equal(7, project.TeacherId);
        Assert.Single(context.Projects);
    }

    [Fact]
    public async Task DeleteProject_WhenProjectDoesNotExist_ReturnsNotFound()
    {
        using var context = TestApplicationContextFactory.Create();
        var controller = CreateController(context);
        ControllerTestHelpers.SetAuthenticatedUser(controller, userId: 1);

        IActionResult result = await controller.DeleteProject(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteProject_WhenTeacherDoesNotOwnProject_ReturnsForbid()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Projects.Add(TestEntities.Project(1, teacherId: 1));
        context.SaveChanges();
        var controller = CreateController(context);
        ControllerTestHelpers.SetAuthenticatedUser(controller, userId: 2);

        IActionResult result = await controller.DeleteProject(1);

        Assert.IsType<ForbidResult>(result);
        Assert.Single(context.Projects);
    }

    [Fact]
    public async Task DeleteProject_WhenTeacherOwnsProject_RemovesProject()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Projects.Add(TestEntities.Project(1, teacherId: 7));
        context.SaveChanges();
        var controller = CreateController(context);
        ControllerTestHelpers.SetAuthenticatedUser(controller, userId: 7);

        IActionResult result = await controller.DeleteProject(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<Project>(ok.Value);
        Assert.Empty(context.Projects);
    }

    [Fact]
    public async Task AddStudentToProject_WhenProjectDoesNotExist_ReturnsNotFound()
    {
        using var context = TestApplicationContextFactory.Create();
        var controller = CreateController(context);
        ControllerTestHelpers.SetAuthenticatedUser(controller, userId: 1);

        ActionResult result = await controller.AddStudentToProject(99, new StudentProjectDTO
        {
            StudentId = 2,
            Role = "Backend"
        });

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Project not found", notFound.Value);
    }

    [Fact]
    public async Task AddStudentToProject_WhenTeacherDoesNotOwnProject_ReturnsForbid()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Projects.Add(TestEntities.Project(1, teacherId: 1));
        context.Students.Add(TestEntities.Student(2));
        context.SaveChanges();
        var controller = CreateController(context);
        ControllerTestHelpers.SetAuthenticatedUser(controller, userId: 7);

        ActionResult result = await controller.AddStudentToProject(1, new StudentProjectDTO
        {
            StudentId = 2,
            Role = "Backend"
        });

        Assert.IsType<ForbidResult>(result);
        Assert.Empty(context.StudentProjects);
    }

    [Fact]
    public async Task AddStudentToProject_WhenStudentDoesNotExist_ReturnsNotFound()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Projects.Add(TestEntities.Project(1, teacherId: 7));
        context.SaveChanges();
        var controller = CreateController(context);
        ControllerTestHelpers.SetAuthenticatedUser(controller, userId: 7);

        ActionResult result = await controller.AddStudentToProject(1, new StudentProjectDTO
        {
            StudentId = 2,
            Role = "Backend"
        });

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Student not found", notFound.Value);
    }

    [Fact]
    public async Task AddStudentToProject_WhenStudentIsAlreadyLinked_ReturnsBadRequest()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Projects.Add(TestEntities.Project(1, teacherId: 7));
        context.Students.Add(TestEntities.Student(2));
        context.StudentProjects.Add(TestEntities.StudentProject(projectId: 1, studentId: 2));
        context.SaveChanges();
        var controller = CreateController(context);
        ControllerTestHelpers.SetAuthenticatedUser(controller, userId: 7);

        ActionResult result = await controller.AddStudentToProject(1, new StudentProjectDTO
        {
            StudentId = 2,
            Role = "Backend"
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Student already in Project", badRequest.Value);
    }

    [Fact]
    public async Task AddStudentToProject_WhenDataIsValid_AddsRelation()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Projects.Add(TestEntities.Project(1, teacherId: 7));
        context.Students.Add(TestEntities.Student(2));
        context.SaveChanges();
        var controller = CreateController(context);
        ControllerTestHelpers.SetAuthenticatedUser(controller, userId: 7);

        ActionResult result = await controller.AddStudentToProject(1, new StudentProjectDTO
        {
            StudentId = 2,
            Role = "Backend"
        });

        var ok = Assert.IsType<OkObjectResult>(result);
        var relation = Assert.IsType<StudentProject>(ok.Value);
        Assert.Equal("Backend", relation.Role);
        Assert.Single(context.StudentProjects);
    }

    [Fact]
    public async Task DeleteStudentFromProject_WhenProjectDoesNotExist_ReturnsNotFound()
    {
        using var context = TestApplicationContextFactory.Create();
        var controller = CreateController(context);
        ControllerTestHelpers.SetAuthenticatedUser(controller, userId: 7);

        ActionResult result = await controller.DeleteStudentFromProject(99, 2);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Project not found", notFound.Value);
    }

    [Fact]
    public async Task DeleteStudentFromProject_WhenStudentDoesNotExist_ReturnsNotFound()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Projects.Add(TestEntities.Project(1, teacherId: 7));
        context.SaveChanges();
        var controller = CreateController(context);
        ControllerTestHelpers.SetAuthenticatedUser(controller, userId: 7);

        ActionResult result = await controller.DeleteStudentFromProject(1, 2);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Student not found", notFound.Value);
    }

    [Fact]
    public async Task DeleteStudentFromProject_WhenTeacherDoesNotOwnProject_ReturnsForbid()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Projects.Add(TestEntities.Project(1, teacherId: 1));
        context.Students.Add(TestEntities.Student(2));
        context.StudentProjects.Add(TestEntities.StudentProject(projectId: 1, studentId: 2));
        context.SaveChanges();
        var controller = CreateController(context);
        ControllerTestHelpers.SetAuthenticatedUser(controller, userId: 7);

        ActionResult result = await controller.DeleteStudentFromProject(1, 2);

        Assert.IsType<ForbidResult>(result);
        Assert.Single(context.StudentProjects);
    }

    [Fact]
    public async Task DeleteStudentFromProject_WhenStudentIsNotInProject_ReturnsNotFound()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Projects.Add(TestEntities.Project(1, teacherId: 7));
        context.Students.Add(TestEntities.Student(2));
        context.SaveChanges();
        var controller = CreateController(context);
        ControllerTestHelpers.SetAuthenticatedUser(controller, userId: 7);

        ActionResult result = await controller.DeleteStudentFromProject(1, 2);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Student not in project.", notFound.Value);
    }

    [Fact]
    public async Task DeleteStudentFromProject_WhenStudentIsLinked_RemovesRelation()
    {
        using var context = TestApplicationContextFactory.Create();
        context.Projects.Add(TestEntities.Project(1, teacherId: 7));
        context.Students.Add(TestEntities.Student(2));
        context.StudentProjects.Add(TestEntities.StudentProject(projectId: 1, studentId: 2));
        context.SaveChanges();
        var controller = CreateController(context);
        ControllerTestHelpers.SetAuthenticatedUser(controller, userId: 7);

        ActionResult result = await controller.DeleteStudentFromProject(1, 2);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<StudentProject>(ok.Value);
        Assert.Empty(context.StudentProjects);
    }

    private static ProjectsController CreateController(ApplicationContext context)
    {
        var userService = new UserService(
            context,
            new PasswordService(),
            ControllerTestHelpers.CreateJwtConfiguration());
        var projectService = new ProjectService(context, userService);

        ProjectsController result = new ProjectsController(projectService, userService);

        return result;
    }
}
