using Auth.DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.DAL.Databases
{
    public class AuthDbContext : DbContext
    {
        public DbSet<IdentityModel> IdentityModels { get; set; }
        public DbSet<RefreshTokenModel> RefreshTokenModels { get; set; }

        public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===============================
            // 🧩 TABLE: IdentityModel
            // ===============================
            modelBuilder.Entity<IdentityModel>(entity =>
            {
                entity.ToTable("IdentityModels");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Email)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.HasIndex(e => e.Email)
                      .IsUnique(); // 🔒 Email duy nhất

                entity.Property(e => e.PasswordHashing)
                      .IsRequired();

                entity.Property(e => e.Role)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.IsVerified)
                      .HasDefaultValue(false);

                entity.Property(e => e.VerifiedAt)
                      .HasColumnType("datetime2");

                entity.Property(e => e.VerificationCode)
                      .HasMaxLength(20);

                entity.Property(e => e.VerificationExpiry)
                      .HasColumnType("datetime2");
            });

            // ===============================
            // 🔄 TABLE: RefreshTokenModel
            // ===============================
            modelBuilder.Entity<RefreshTokenModel>(entity =>
            {
                entity.ToTable("RefreshTokenModels");

                entity.HasKey(e => e.RefreshTokenId);

                entity.Property(e => e.TokenHash)
                      .IsRequired();

                entity.Property(e => e.ExpiryDate)
                      .IsRequired();

                entity.Property(e => e.SessionExpiry)
                      .IsRequired();

                entity.Property(e => e.LastActivity)
                      .IsRequired();

                // ⚙️ Quan hệ 1-N giữa IdentityModel và RefreshTokenModel
                entity.HasOne<IdentityModel>()
                      .WithMany()
                      .HasForeignKey(rt => rt.IdentityId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(rt => rt.IdentityId);
            });

            // ===============================
            // 🔧 OPTIONAL: Seed dữ liệu mẫu
            // ===============================
            // modelBuilder.Entity<IdentityModel>().HasData(
            //     new IdentityModel
            //     {
            //         Id = Guid.NewGuid(),
            //         UserId = Guid.NewGuid(),
            //         Email = "admin@auth.local",
            //         PasswordHashing = "dummy-hash",
            //         Role = "admin",
            //         IsVerified = true,
            //         VerifiedAt = DateTime.UtcNow
            //     }
            // );
        }
    }
}
