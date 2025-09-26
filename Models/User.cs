using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AspNetDemoPortalAPI.Models
{
    [Table("AspNetUsers")]
    public class User
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string Role { get; set; } = "User";

        public string Username { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
