using Frontend.Cache;
using Frontend.Cache.Interfaces;
using Frontend.Configs.Auth;
using Frontend.Configs.Order;
using Frontend.Configs.Product;
using Frontend.Configs.Store;
using Frontend.Helpers;
using Frontend.HttpsClients.Auths;
using Frontend.HttpsClients.Orders;
using Frontend.HttpsClients.Products;
using Frontend.HttpsClients.Stores;
using Frontend.Middlewares;
using Frontend.Services;
using Frontend.Services.Interfaces;
using Microsoft.Extensions.FileProviders;
using StackExchange.Redis;
using System.Text.Json;


namespace Frontend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Setting Helper
            SettingsHelper.Configure(builder.Configuration);
            builder.Services.AddHttpContextAccessor();

            // Connect Auth Service
            builder.Services.AddScoped<HeaderHandler>();

            // Order Enpoints
            builder.Services.AddHttpClient<IOrderApiClient, OrderApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Ocelot:BaseUrl"]);
            }).AddHttpMessageHandler<HeaderHandler>();

            builder.Services.Configure<OrderEndpoints>(
                builder.Configuration.GetSection("Ocelot:ServiceUrls:Order:Endpoints"));

            // Auth Enpoints
            builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Ocelot:BaseUrl"]);
            });

            builder.Services.Configure<AuthEndpoints>(
                builder.Configuration.GetSection("Ocelot:ServiceUrls:Auth:Endpoints"));

            // Store endpoints
            builder.Services.AddHttpClient<IStoreApiClient, StoreApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Ocelot:BaseUrl"]);
            }).AddHttpMessageHandler<HeaderHandler>();

            builder.Services.Configure<StoreEndpoints>(
                builder.Configuration.GetSection("Ocelot:ServiceUrls:Store:Endpoints"));

            // Product Endpoints
            builder.Services.AddHttpClient<IProductApiClient, ProductApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Ocelot:BaseUrl"]);
            }).AddHttpMessageHandler<HeaderHandler>();

            builder.Services.Configure<ProductEndpoints>(
                builder.Configuration.GetSection("Ocelot:ServiceUrls:Product:Endpoints"));

            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IStoreService, StoreService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IOrderService, OrderService>();  

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // session timeout
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Redis Cache
            builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var configuration = builder.Configuration.GetConnectionString("Redis");
                return ConnectionMultiplexer.Connect(configuration);
            });

            // Static File

            // Đăng ký RedisCacheService
            builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

            //JWT Authentication

            builder.Services.AddAuthorization();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseRouting();
            app.UseSession();

            app.UseMiddleware<SessionRestoreMiddleware>();
            app.UseMiddleware<RefreshTokenMiddleware>();

            app.UseHttpsRedirection();
            app.UseStaticFiles();


            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
