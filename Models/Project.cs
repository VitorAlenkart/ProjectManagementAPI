using Microsoft.VisualBasic;

namespace ProjectManagementAPI.Models
{
    public class Project
    {
        public int Id { get; set; }
        public required string name { get; set; }
        public required string description { get; set; }
        public required DateFormat dateFormat { get; set; }
        public required Teacher coordinator { get; set; }
        public List<Student> students { get; set; } = new();
    }
}
