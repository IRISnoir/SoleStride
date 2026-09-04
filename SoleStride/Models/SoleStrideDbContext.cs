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
        public DbSet<OrderStock> OrderStocks { get; set; }

        // Address Book
        public DbSet<Address> Addresses { get; set; }

        public Order Order
        {
            get => default;
            set
            {
            }
        }

        public ShoeStock ShoeStock
        {
            get => default;
            set
            {
            }
        }

        public OrderDetail OrderDetail
        {
            get => default;
            set
            {
            }
        }

        public OrderStock OrderStock
        {
            get => default;
            set
            {
            }
        }

        public User User
        {
            get => default;
            set
            {
            }
        }

        public Shoes Shoes1
        {
            get => default;
            set
            {
            }
        }
    }
}
