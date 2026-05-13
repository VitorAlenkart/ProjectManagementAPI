using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementAPI.Models
{
    public class Student : User
    {
        [Required]
        public string EducationalInstitution { get; set; }
        public List<StudentProject> StudentProjects { get; set; } = new List<StudentProject>();
        [NotMapped]
        public string Role { get; set; }

    }
}
