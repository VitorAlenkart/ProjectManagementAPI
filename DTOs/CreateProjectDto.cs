using System.ComponentModel.DataAnnotations;

namespace ProjectManagementAPI.DTOs
{
    public class CreateProjectDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int TeacherId { get; set; }
    }
}