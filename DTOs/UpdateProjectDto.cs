using System.ComponentModel.DataAnnotations;
namespace ProjectManagementAPI.DTOs
{
    public class UpdateProjectDto
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
