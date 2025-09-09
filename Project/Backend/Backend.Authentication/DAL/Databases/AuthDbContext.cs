using DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Databases
{
    public class AuthDbContext : DbContext
    {
        public DbSet<IdentityModel> IdentityModels { get; set; }
        public DbSet<RefreshTokenModel> RefreshTokenModels { get; set; }

        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // IdentityModel
            modelBuilder.Entity<IdentityModel>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Email)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(e => e.PasswordHashing)
                      .IsRequired();

                entity.Property(e => e.Role)
                      .IsRequired()
                      .HasMaxLength(50);
            });

            // RefreshTokenModel
            modelBuilder.Entity<RefreshTokenModel>(entity =>
            {
                entity.HasKey(e => e.RefreshTokenId);

                entity.Property(e => e.TokenHash).IsRequired();
                entity.Property(e => e.ExpiryDate).IsRequired();
                entity.Property(e => e.SessionExpiry).IsRequired();
                entity.Property(e => e.LastActivity).IsRequired();

                // Quan hệ 1-n: Một user có nhiều refresh token
                entity.HasOne<IdentityModel>()
                      .WithMany()
                      .HasForeignKey(rt => rt.IdentityId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(rt => rt.IdentityId);
            });
        }
    }
}
