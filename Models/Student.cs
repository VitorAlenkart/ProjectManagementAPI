namespace ProjectManagementAPI.Models
{
    public class Student : User
    {

        public string? EducationalInstitution { get; set; }

        public List<StudentProject> StudentProjects { get; set; } = new();
    }
}
