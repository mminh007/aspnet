using Backend.Authentication.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Backend.Authentication.Databases
{
    public class AuthDbContext : DbContext
    {
        public DbSet<IdentityModel> IdentityModels { get; set; }
        public DbSet<RefreshTokenModel> refreshTokenModels { get; set; }

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

                entity.Property(e => e.TokenHash)
                      .IsRequired();

                entity.Property(e => e.ExpiryDate)
                      .IsRequired();

                // Quan hệ 1-1: IdentityModel ↔ RefreshTokenModel
                entity.HasOne<IdentityModel>()
                      .WithOne()
                      .HasForeignKey<RefreshTokenModel>(rt => rt.IdentityId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
