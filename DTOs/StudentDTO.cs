using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementAPI.DTOs
{
    public class StudentDTO
    {

        public StudentDTO() { }
        public int Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }      
        public string EducationalInstitution { get; set; }
        public string Role { get; set; }
    }
}
