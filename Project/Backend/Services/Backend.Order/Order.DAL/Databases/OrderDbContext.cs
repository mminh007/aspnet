using Microsoft.EntityFrameworkCore;
using Order.DAL.Models.Entities;

namespace Order.DAL.Databases
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options)
            : base(options)
        {
        }

        // DbSet cho các entity
        public DbSet<CartModel> Carts { get; set; }
        public DbSet<CartItemModel> CartItems { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<OrderItemModel> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cart ↔ CartItem (1-n)
            modelBuilder.Entity<CartModel>()
                .HasMany(c => c.Items)
                .WithOne(i => i.Cart)
                .HasForeignKey(i => i.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order ↔ OrderItem (1-n)
            modelBuilder.Entity<OrderModel>()
                .HasMany(o => o.OrderItems)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // ✅ Default value cho UpdatedAt
            modelBuilder.Entity<OrderModel>()
                .Property(o => o.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Decimal precision cho Price và TotalAmount
            modelBuilder.Entity<CartItemModel>()
                .Property(i => i.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CartItemModel>()
                .Property(i => i.ProductImage)
                .HasMaxLength(500);

            modelBuilder.Entity<OrderItemModel>()
                .Property(i => i.Price)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<OrderModel>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            // ✅ Cấu hình cho OrderName
            modelBuilder.Entity<OrderModel>()
                .Property(o => o.OrderName)
                .IsRequired()
                .HasMaxLength(100);


        }
    }
}
