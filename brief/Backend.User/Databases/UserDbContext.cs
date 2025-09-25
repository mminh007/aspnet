using Backend.User.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.User.Databases
{
    public class UserDbContext: DbContext
    {
        public virtual DbSet<UserModel> Users { get; set; }

        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserModel>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.PhoneNumber).IsUnique();
            });
        }
    }
}
