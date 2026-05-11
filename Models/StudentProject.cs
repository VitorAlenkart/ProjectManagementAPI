namespace ProjectManagementAPI.Models
{
    public class StudentProject
    {
        public required int StudentId { get; set; }
        public Student? Student { get; set; }

        public required int ProjectId { get; set; }
        public Project? Project { get; set; }

        public required string Role { get; set; }
    }
}
