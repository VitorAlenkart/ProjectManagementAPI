using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.Utils;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementAPI.Models;

    public abstract class User
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public required string Email { get; set; }
        public required string HashedPassword { get; set; }
        public List<Project> Projects { get; } = new List<Project>();
    }

