using System.ComponentModel.DataAnnotations;

namespace ProjectManagementAPI.DTOs
{
    public class CreateStudentDto
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string EducationalInstitution { get; set; }
     }
}
