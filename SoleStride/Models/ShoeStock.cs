using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoleStride.Models
{
    public class ShoeStock
    {
        [Key]
        public int StockId { get; set; }

        [Required]
        public Guid ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Shoes Shoes { get; set; }

        public enum InventoryStatus
        {
            Available,
            Sold,
            Damaged
        }
        [Required]
        public InventoryStatus Status { get; set; }

        [Required]
        public DateTime EntryDate { get; set; } = DateTime.Now;

        public DateTime? PurchaseDate { get; set; }
    }
}
