namespace ProjectManagementAPI.Models
{
    public class Teacher : User
    { 
        public required string occupationArea { get; set; }
        public required string formationArea { get; set; }
    }
}
