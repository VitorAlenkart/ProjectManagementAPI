using Microsoft.EntityFrameworkCore;
using ProjectManagementAPI.Data;

namespace ProjectManagementAPI.UnitTests.TestSupport;

internal static class TestApplicationContextFactory
{
    public static ApplicationContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}
