using System.ComponentModel.DataAnnotations;

namespace ProjectManagementAPI.Models
{
    public class StudentProject
    {
        [Key]
        public required int StudentId { get; set; }
        [Key]
        public required int ProjectId { get; set; }
        public Student student { get; set; }
        public Project project { get; set; }

        public required string Role { get; set; }
    }
}
