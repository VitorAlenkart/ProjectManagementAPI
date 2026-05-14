using ProjectManagementAPI.Models;

namespace ProjectManagementAPI.DTOs
{
    public class DetailedProjectDTO
    {
        public DetailedProjectDTO() { }
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public DateTime date { get; set; }
        public int teacherId { get; set; }
        public List<StudentDTO> students { get; set; } = new List<StudentDTO>();

    }
}
