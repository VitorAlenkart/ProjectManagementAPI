using System.ComponentModel.DataAnnotations;

namespace ProjectManagementAPI.Models
{
    public class Teacher : User
    {
        [Required]
        public string OccupationArea { get; set; }
        [Required]
        public string FormationArea { get; set; }
    }
}
