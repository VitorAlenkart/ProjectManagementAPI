using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;

namespace ProjectManagementAPI.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string Description { get; set; }
        [Required]
        public required DateTime Date { get; set; }
        [Required]
        public int TeacherId { get; set; }
        public List<StudentProject> StudentProjects { get; set; } = new();
    }
}
