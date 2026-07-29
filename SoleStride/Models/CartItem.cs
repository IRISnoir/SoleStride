namespace SoleStride.Models
{
    public class CartItem
    {
        public Guid ProductId { get; set; }
        public string ShoesName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public float? SalePercentage { get; set; }
        public int Quantity { get; set; }

        public decimal FinalPrice => SalePercentage > 0 ? Price * (1 - (decimal)(SalePercentage / 100)) : Price;
        public decimal Subtotal => FinalPrice * Quantity;
    }
}
