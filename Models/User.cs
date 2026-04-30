using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementAPI.Models;

    public abstract class User
    {
        public int id { get; set; }
        public required string fullName { get; set; }
        public required string email { get; set; }
        public required string hashedPassword { get; set; }
    }

