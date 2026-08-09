using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoleStride.Models
{
    public class OrderStock
    {
        [Key]
        public int OrderStockId { get; set; }

        [Required]
        public int OrderDetailId { get; set; }

        [ForeignKey(nameof(OrderDetailId))]
        public OrderDetail? OrderDetail { get; set; }

        [Required]
        public int StockId { get; set; }

        [ForeignKey(nameof(StockId))]
        public ShoeStock? ShoeStock { get; set; }
    }
}
