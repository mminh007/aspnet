using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Databases
{
    public class StoreDbContext : DbContext
    {
        public DbSet<StoreModel> Stores { get; set; }

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

                entity.Property(e => e.StoreName)
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
        }
        

    }
}
