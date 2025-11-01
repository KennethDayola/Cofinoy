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
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

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

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.UnitPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CartItem>()
                .Property(ci => ci.TotalPrice)
                .HasPrecision(18, 2);



            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .HasColumnName("OrderId");

                entity.Property(e => e.TotalPrice)
                      .HasPrecision(18, 2);

                entity.Property(e => e.OrderDate)
                      .IsRequired();

            
                entity.HasMany(e => e.OrderItems)
                      .WithOne(e => e.Order)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .HasColumnName("OrderItemId"); 

                entity.Property(e => e.UnitPrice)
                      .HasPrecision(18, 2);

                entity.Property(e => e.TotalPrice)
                      .HasPrecision(18, 2);

                // Relationship with Order
                entity.HasOne(e => e.Order)
                      .WithMany(e => e.OrderItems)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // Temporarily remove Menu->Category relationship until DB migration adds MenuId
        }

  




    }



}
