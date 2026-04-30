using Microsoft.VisualBasic;

namespace ProjectManagementAPI.Models
{
    public class Project
    {
        public int id { get; set; }
        public required string name { get; set; }
        public required string description { get; set; }
        public required DateTime date { get; set; }

        public int teacherId { get; set; }
        public required Teacher teacher { get; set; }
        public List<Student> students { get;} = new();
    }
}
