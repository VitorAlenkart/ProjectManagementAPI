using Microsoft.VisualBasic;

namespace ProjectManagementAPI.Models
{
    public class Project
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public DateTime Date { get; set; }
        public int TeacherId { get; set; }

        public List<StudentProject> StudentProjects { get; set; } = new();
    }
}
