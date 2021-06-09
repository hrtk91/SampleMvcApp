using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SampleMvcApp.Models;

    public class SampleMVCAppContext : DbContext
    {
        public SampleMVCAppContext (DbContextOptions<SampleMVCAppContext> options)
            : base(options)
        {
        }

        public DbSet<SampleMvcApp.Models.Product> Product { get; set; }

        public DbSet<SampleMvcApp.Models.Genre> Genre { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<ProductGenre>()
                .HasKey(c => new { c.ProductId, c.GenreId });
        }
    }
