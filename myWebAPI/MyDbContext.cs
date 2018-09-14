using Microsoft.EntityFrameworkCore;
using myWebAPI.Controllers;

namespace myWebAPI
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }

        public DbSet<CityName> CityNames { get; set; }
    }
}
