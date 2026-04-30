namespace ProjectManagementAPI.Models
{
    public class StudentProject
    {
        public required int studentId { get; set; }
        public required Student student { get; set; }

        public required int projectId { get; set; }
        public required Project project { get; set; }
    }
}
