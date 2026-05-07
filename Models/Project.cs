using Microsoft.VisualBasic;

namespace ProjectManagementAPI.Models
{
    public class Project
    {
        public int id { get; set; }
        public required string name { get; set; }
        public required string description { get; set; }
        public DateTime date { get; set; }

        public int teacherId { get; set; }
        public List<Student> students { get; set; } = new();
    }
}
