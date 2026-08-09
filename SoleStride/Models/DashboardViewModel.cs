using SoleStride.Models;

namespace SoleStride.Models
{
    public class DashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalUsers { get; set; }

        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }

        public int StockAvailable { get; set; }
        public int StockSold { get; set; }
        public int StockDamaged { get; set; }

        public List<Order> RecentOrders { get; set; } = new();
        public List<BestSellerItem> BestSellers { get; set; } = new();
        public List<CategorySaleItem> SalesByCategory { get; set; } = new();
        public List<Shoes> LowStockProducts { get; set; } = new();
        public Dictionary<Guid, int> AvailableCounts { get; set; } = new();
        public string? SelectedMonth { get; set; }
        public List<User> Users { get; set; } = new();
    }

    public class BestSellerItem
    {
        public Guid ProductId { get; set; }
        public string ShoesName { get; set; }
        public string CategoryName { get; set; }
        public decimal Price { get; set; }
        public int TotalSold { get; set; }
    }

    public class CategorySaleItem
    {
        public string CategoryName { get; set; }
        public decimal Revenue { get; set; }
    }
}
