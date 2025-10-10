using Store.DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Store.DAL.Databases
{
    public class StoreDbContext : DbContext
    {
        public DbSet<StoreModel> Stores { get; set; }

        public DbSet<IntegrationEventLog> IntegrationEventLogs { get; set; }

        public StoreDbContext(DbContextOptions<StoreDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<StoreModel>(entity =>
            {
                entity.ToTable("Stores");

                // Primary Key
                entity.HasKey(e => e.StoreId);

                entity.HasIndex(e => e.UserId);

                entity.HasIndex(e => e.StoreCategorySlug); // ✅ index cho tìm kiếm slug

                entity.Property(e => e.StoreName)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.StoreImage)
                      .HasMaxLength(255)
                      .IsRequired(false); ;

                entity.Property(e => e.StoreCategory)
                      .HasMaxLength(50);

                entity.Property(e => e.Phone)
                      .HasMaxLength(10);

                entity.Property(e => e.Description)
                      .HasMaxLength(500);

                entity.Property(e => e.IsActive)
                      .HasDefaultValue(true);

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.UpdatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            // IntegrationEventLog
            modelBuilder.Entity<IntegrationEventLog>(entity =>
            {
                entity.ToTable("IntegrationEventLogs");
                entity.HasKey(e => e.EventId);
                entity.Property(e => e.EventType).IsRequired().HasMaxLength(250);
            });
        }
    }
}
