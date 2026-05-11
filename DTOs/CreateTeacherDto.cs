using System.ComponentModel.DataAnnotations;

namespace ProjectManagementAPI.DTOs
{
    public class CreateTeacherDto
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string OccupationArea { get; set; }

        [Required]
        public string FormationArea { get; set; }
    }
}
