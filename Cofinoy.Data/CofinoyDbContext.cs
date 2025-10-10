using Cofinoy.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Cofinoy.Data
{
    public class CofinoyDbContext : DbContext
    {
        public CofinoyDbContext(DbContextOptions<CofinoyDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<Customization> Customizations { get; set; }
        public DbSet<CustomizationOption> CustomizationOptions { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductCustomization> ProductCustomizations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customization>()
                .Property(c => c.PricePerUnit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CustomizationOption>()
                .Property(co => co.PriceModifier)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.BasePrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ProductCategory>()
                .HasKey(pc => new { pc.ProductId, pc.CategoryId });

            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p.ProductCategories)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductCategory>()
                .HasOne(pc => pc.Category)
                .WithMany(c => c.ProductCategories)
                .HasForeignKey(pc => pc.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductCustomization>()
                .HasKey(pc => new { pc.ProductId, pc.CustomizationId });

            modelBuilder.Entity<ProductCustomization>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p.ProductCustomizations)
                .HasForeignKey(pc => pc.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductCustomization>()
                .HasOne(pc => pc.Customization)
                .WithMany(c => c.ProductCustomizations)
                .HasForeignKey(pc => pc.CustomizationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CustomizationOption>()
                .HasOne(co => co.Customization)
                .WithMany(c => c.Options)
                .HasForeignKey(co => co.CustomizationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Temporarily remove Menu->Category relationship until DB migration adds MenuId
        }
    }
}
