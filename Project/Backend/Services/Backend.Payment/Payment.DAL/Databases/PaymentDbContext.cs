using Microsoft.EntityFrameworkCore;
using Payment.DAL.Models.Entities;

namespace Payment.DAL
{
    public class PaymentDbContext : DbContext
    {
        public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
            : base(options)
        {
        }

        public DbSet<PaymentModel> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PaymentModel>(entity =>
            {
                entity.ToTable("Payments");

                entity.HasKey(e => e.PaymentId);

                entity.Property(e => e.OrderId).IsRequired();
                entity.Property(e => e.BuyerId).IsRequired();
                entity.Property(e => e.Method).IsRequired();

                entity.Property(e => e.Amount)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                entity.Property(e => e.Currency)
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(e => e.TransactionId).HasMaxLength(100);
                entity.Property(e => e.GatewayResponse);
                entity.Property(e => e.FailureReason).HasMaxLength(500);

                entity.Property(e => e.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.Status)
                      .HasConversion<int>()
                      .IsRequired();
            });
        }
    }
}
