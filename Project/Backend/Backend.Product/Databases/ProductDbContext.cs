using Backend.Product.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Product.Databases
{
    public class ProductDbContext : DbContext
    {
        public DbSet<ProductModel> Products { get; set; }

        public DbSet<CategoryModel> Category { get; set; }

        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductModel>(entity =>
            {
                entity.ToTable("Products");

                // Primary Key
                entity.HasKey(e => e.ProductId);

                entity.HasIndex(e => e.StoreId);

                entity.Property(e => e.ProductName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Description)
                      .HasMaxLength(500);

                entity.Property(e => e.IsActive)
                      .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

            });

            modelBuilder.Entity<ProductModel>()
                .HasOne(p => p.Category)
                .WithMany(p => p.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CategoryModel>(entity =>
            {
                entity.ToTable("Categories");

                entity.HasKey(e => e.CategoryId);

                entity.Property(e => e.CategoryName)
                      .IsRequired()
                      .HasMaxLength(50);
            });
        }
    }
}
