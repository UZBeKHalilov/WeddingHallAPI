using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WeddingHallAPI.Models;

namespace WeddingHallAPI.Data
{
    public class WeddingHallDbContext : DbContext
    {
        public WeddingHallDbContext(DbContextOptions<WeddingHallDbContext> options) : base(options)
        {
        }
        public DbSet<WeddingHall> WeddingHalls { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Food> Foods { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseInMemoryDatabase("WeddingHallDb");
            }
            base.OnConfiguring(optionsBuilder);
        }
    }
}
