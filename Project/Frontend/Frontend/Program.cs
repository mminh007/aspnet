using Frontend.HttpsClients.Auths;
using Frontend.HttpsClients.Stores;

namespace Frontend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Connect Auth Service
            builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Ocelot:BaseUrl"]); // Ocelot Gateway
            });

            // Connect Store Service
            builder.Services.AddHttpClient<IStoreApiClient, StoreApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Ocelot:BaseUrl"]);
            });

            builder.Services.AddHttpClient<IStoreApiClient, StoreApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Ocelot:BaseUrl"]);
            });

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // session timeout
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
