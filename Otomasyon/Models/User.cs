using System.ComponentModel.DataAnnotations;

namespace Otomasyon.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }

        public DateTime CreatedDate { get; set; }

        public ICollection<Project> Projects { get; set; }
    }
} 