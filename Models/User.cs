using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.Utils;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementAPI.Models;

    public abstract class User
    {
        public int id { get; set; }
        public required string fullName { get; set; }
        public required string email { get; set; }
        public required string hashedPassword { get; set; }
        public List<Project> projects { get; } = new List<Project>();
    }

