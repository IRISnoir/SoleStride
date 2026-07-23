using Microsoft.EntityFrameworkCore;

namespace SoleStride.Models
{
    public class SoleStrideDbContext : DbContext
    {
        public SoleStrideDbContext(DbContextOptions<SoleStrideDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Shoes> Shoes { get; set; }
        public DbSet<ShoeStock> ShoeStocks { get; set; }
    }
}
