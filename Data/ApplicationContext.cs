using Microsoft.EntityFrameworkCore;
using ProjectManagementAPI.Models;

namespace ProjectManagementAPI.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext() {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<User>();

            modelBuilder.Entity<Project>()
                .HasOne(p => p.teacher)              
                .WithMany(t => t.projects)     
                .OnDelete(DeleteBehavior.Restrict);
        }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {}

        public DbSet<Project> Projects { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
    }
}
