using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SampleMvcApp.Models;

namespace SampleMvcApp.Data
{
    public class SampleMVCAppContext : IdentityDbContext
    {
        public SampleMVCAppContext (DbContextOptions<SampleMVCAppContext> options)
            : base(options)
        {
        }

        public DbSet<SampleMvcApp.Models.Product> Product { get; set; }

        public DbSet<SampleMvcApp.Models.Genre> Genre { get; set; }

        public DbSet<SampleMvcApp.Models.Shop> Shop { get; set; }

        public DbSet<SampleMvcApp.Models.Cart> Cart { get; set; }

        public DbSet<SampleMvcApp.Models.Receipt> Receipt { get; set; }

        public DbSet<SampleMvcApp.Models.ProductImage> ProductImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            CreateAdminRole(modelBuilder);
        }

        private void CreateAdminRole(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole("Admin") { NormalizedName = "Admin".ToUpper() },
                new IdentityRole("Seller") { NormalizedName = "Seller".ToUpper() },
                new IdentityRole("User") { NormalizedName = "User".ToUpper() }
            );
        }
    }
}
