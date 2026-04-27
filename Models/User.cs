using Microsoft.EntityFrameworkCore;

namespace ProjectManagementAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string HashedPassword { get; set; }
    }
}
