using Adminstrator.Configs.Auth;
using Adminstrator.Configs.Product;
using Adminstrator.Configs.Store;
using Adminstrator.Helpers;
using Adminstrator.HttpsClients;
using Adminstrator.HttpsClients.Auths;
using Adminstrator.HttpsClients.Interfaces;
using Adminstrator.Middlewares;
using Adminstrator.Services;
using Adminstrator.Services.Interfaces;
using Microsoft.Extensions.FileProviders;


namespace Adminstrator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            DotNetEnv.Env.Load($".env.{env.ToLower()}");

            var storeImagePath = Environment.GetEnvironmentVariable("STORE_IMAGE_PATH");
            var storeImageRequest = Environment.GetEnvironmentVariable("STORE_IMAGE_REQUEST");           

            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddEnvironmentVariables();

            SettingsHelper.Configure(builder.Configuration);

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<HeaderHandler>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Connect API Product Backend Service
            builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
            {
                var baseUrl = builder.Configuration["OCELOT:BASEURL"];
                client.BaseAddress = new Uri(baseUrl);
                Console.WriteLine($"OCELOT BaseURL: {baseUrl}");
            });
            

            builder.Services.Configure<AuthEndpoints>(
                builder.Configuration.GetSection("OCELOT:SERVICEURLS:AUTH:ENDPOINTS"));

            builder.Services.AddHttpClient<IStoreApiClient, StoreApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["OCELOT:BASEURL"]);
            }).AddHttpMessageHandler<HeaderHandler>();

            builder.Services.Configure<StoreEndpoints>(
                builder.Configuration.GetSection("OCELOT:SERVICEURLS:STORE:ENDPOINTS"));

            builder.Services.AddHttpClient<IProductApiClient, ProductApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["OCELOT:BASEURL"]);
            }).AddHttpMessageHandler<HeaderHandler>();

            builder.Services.Configure<ProductEndpoints>(
                builder.Configuration.GetSection("OCELOT:SERVICEURLS:PRODUCT:ENDPOINTS"));

            // Connect Auth Service

            builder.Services.AddScoped<IStoreService, StoreServices>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            // Connect Service

            // map static files
            builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // session timeout
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });


            builder.Services.AddAuthorization();

            //builder.Services.AddAuthorization();

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

            if (!string.IsNullOrEmpty(storeImagePath) && !string.IsNullOrEmpty(storeImageRequest))
            {
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(storeImagePath),
                    RequestPath = storeImageRequest
                });
            }


            app.UseRouting();
            app.UseSession();

            app.UseTokenToHeader();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Authentication}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
