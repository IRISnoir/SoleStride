using System.ComponentModel.DataAnnotations;

namespace SoleStride.Models
{
    public class Address
    {
        [Key]
        public int AddressId { get; set; }

        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Recipient name is required.")]
        [MaxLength(100)]
        public string RecipientName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required.")]
        [MaxLength(500)]
        public string AddressLine { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;
    }
}