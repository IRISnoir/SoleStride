using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SoleStride.Models
{
    public class User
    {
        [Key]
        [MaxLength(100)]
        public string Username { get; set; }

        [PasswordPropertyText]
        [Required]
        public string Password { get; set; }

        public enum UserRole
        {
            Admin,
            Staff,
            User
        }
        [Required]
        public UserRole Role { get; set; }

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [Phone]
        public string? Phone { get; set; }

        [EmailAddress]
        [Required]
        public string? EmailAddress { get; set; }

        [BirthdateConstraint]
        [Required]
        public DateTime Birthdate { get; set; } = DateTime.Now - TimeSpan.FromDays(14 * 365);

        public enum Gender { Male, Female, Other }
        [Required]
        public Gender UserGender { get; set; }

        public string? AvatarUrl { get; set; }
    }
}
