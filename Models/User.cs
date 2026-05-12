using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.Utils;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProjectManagementAPI.Models;

    public abstract class User
    {
        [Key] 
        public int Id{ get; set; }
        [Required]
        public required string FullName { get; set; }
        [Required]
        public required string Email { get; set; }
        public required string HashedPassword { get; set; }
    }

