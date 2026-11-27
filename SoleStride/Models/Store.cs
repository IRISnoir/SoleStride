using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoleStride.Models
{
    public class Store
    {
        public int StoreId { get; set; }

        [Required]
        [StringLength(100)]
        public string StoreName { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(15)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(9,6)")]
        public decimal Latitude { get; set; }

        [Required]
        [Column(TypeName = "decimal(9,6)")]
        public decimal Longitude { get; set; }

        [StringLength(100)]
        public string? OpeningHours { get; set; }
    }
}
