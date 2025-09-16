
using User.DAL.Databases;
using Store.DAL.Databases;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ToolDB
{

    class Program
    {
        static async Task Main(string[] args)
        {
            var userOptions = new DbContextOptionsBuilder<UserDbContext>()
                .UseSqlServer("Server=localhost;Database=WebService.UserDB;Trusted_Connection=True;TrustServerCertificate=True")
                .Options;

            var storeOptions = new DbContextOptionsBuilder<StoreDbContext>()
                .UseSqlServer("Server=localhost;Database=WebService.StoreDB;Trusted_Connection=True;TrustServerCertificate=True")
                .Options;

            try
            {
                using var userDb = new UserDbContext(userOptions);
                using var storeDb = new StoreDbContext(storeOptions);

                var stores = await storeDb.Stores.ToListAsync();

                foreach (var store in stores)
                {
                    var user = await userDb.Users.FirstOrDefaultAsync(u => u.UserId == store.UserId);
                    if (user != null)
                    {
                        user.StoreId = store.StoreId;
                    }
                }

                await userDb.SaveChangesAsync();
                Console.WriteLine("✅ Sync StoreId vào Users thành công!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi sync: {ex}");
            }

        }
    }

}
