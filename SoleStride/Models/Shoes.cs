using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace SoleStride.Models
{
    public class Shoes
    {
        [Key]
        public Guid ProductId { get; set; }

        [Required]
        [StringLength(100)]
        public string ShoesName { get; set; }

        [Required]
        [StringLength(20)]
        public string SkuId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        [StringLength(10)]
        [Required]
        public string CategoryId { get; set; }

        public enum Gender
        {
            Men,
            Women,
            Unisex
        }
        [Required]
        public Gender ShoesGender { get; set; }

        [Required]
        public int ShoesSize { get; set; }

        [Required]
        public string Material { get; set; }

        public string? Description { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        public float? SalePercentage { get; set; }
    }
}
