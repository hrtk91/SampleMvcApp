using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SampleMvcApp.Models;

    public class SampleMVCAppContext : IdentityDbContext
    {
        public SampleMVCAppContext (DbContextOptions<SampleMVCAppContext> options)
            : base(options)
        {
        }

        public DbSet<SampleMvcApp.Models.Product> Product { get; set; }

        public DbSet<SampleMvcApp.Models.Genre> Genre { get; set; }

        public DbSet<SampleMvcApp.Models.Shop> Shop { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder
                .Entity<ProductGenre>()
                .HasKey(c => new { c.ProductId, c.GenreId });
        }
    }
