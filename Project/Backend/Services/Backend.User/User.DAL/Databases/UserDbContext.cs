using User.DAL.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace User.DAL.Databases
{
    public class UserDbContext : DbContext
    {
        public virtual DbSet<UserModel> Users { get; set; }

        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserModel>(entity =>
            {
                entity.HasKey(e => e.UserId);

                // Email bắt buộc, unique
                entity.HasIndex(e => e.Email).IsUnique();

                // PhoneNumber có thể null nhưng nếu có thì unique
                entity.HasIndex(e => e.PhoneNumber).IsUnique();

                // StoreId: cho phép null (nullable), tạo index để join nhanh hơn
                entity.HasIndex(e => e.StoreId);
            });
        }
    }
}
