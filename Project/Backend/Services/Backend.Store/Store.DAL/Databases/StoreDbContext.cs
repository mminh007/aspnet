using Store.DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Store.DAL.Databases
{
    public class StoreDbContext : DbContext
    {
        public DbSet<StoreModel> Stores { get; set; }
        public DbSet<AccountBanking> AccountBankings { get; set; } 
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

            // AccountBanking
            modelBuilder.Entity<AccountBanking>(entity =>
            {
                entity.ToTable("AccountBankings");
                entity.HasKey(e => e.StoreId);

                entity.Property(e => e.BankName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.AccountNumber)
                      .IsRequired()
                      .HasMaxLength(40);

                entity.Property(e => e.Balance)
                      .HasColumnType("decimal(18,2)");

                // (BankName, AccountNumber) duy nhất trong hệ thống
                entity.HasIndex(e => new { e.BankName, e.AccountNumber }).IsUnique();

                // 1–1: Store <-> AccountBanking (PK = FK)
                entity.HasOne(e => e.Store)
                      .WithOne(s => s.AccountBanking)
                      .HasForeignKey<AccountBanking>(e => e.StoreId)
                      .OnDelete(DeleteBehavior.Cascade); // Xóa Store sẽ xóa luôn AccountBanking
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
