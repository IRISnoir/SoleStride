using System.ComponentModel.DataAnnotations;

namespace SoleStride.Models
{
    public class Category
    {
        [Key]
        [StringLength(10)]
        public string CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; }
    }
}
