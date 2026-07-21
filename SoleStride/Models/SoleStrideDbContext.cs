using Microsoft.EntityFrameworkCore;

namespace SoleStride.Models
{
    public class SoleStrideDbContext : DbContext
    {
        public SoleStrideDbContext(DbContextOptions<SoleStrideDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
    }
}
