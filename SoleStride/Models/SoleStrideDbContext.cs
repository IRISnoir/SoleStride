using Microsoft.EntityFrameworkCore;

namespace SoleStride.Models
{
    public class SoleStrideDbContext : DbContext
    {
        public DbSet<SoleStride.Models.Category> Category { get; set; } = default!;
        public SoleStrideDbContext(DbContextOptions<SoleStrideDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Shoes> Shoes { get; set; }
        public DbSet<ShoeStock> ShoeStocks { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
    }
}
