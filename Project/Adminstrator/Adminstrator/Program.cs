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
using Serilog;

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

            // Cho phép đọc biến môi trường
            builder.Configuration.AddEnvironmentVariables();

            // ==========================================================
            // 🧾 Serilog: đọc cấu hình từ appsettings + enrich context
            // ==========================================================
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();
            builder.Host.UseSerilog();

            try
            {
                Log.Information("Starting Adminstrator with environment: {Env}", env);
                if (!string.IsNullOrEmpty(storeImagePath) || !string.IsNullOrEmpty(storeImageRequest))
                {
                    Log.Information("Static images mapping => PATH: {Path}, REQUEST: {Req}", storeImagePath, storeImageRequest);
                }

                // Binding cấu hình custom
                SettingsHelper.Configure(builder.Configuration);

                builder.Services.AddHttpContextAccessor();
                builder.Services.AddScoped<HeaderHandler>();

                // MVC
                builder.Services.AddControllersWithViews();

                // HTTP Clients tới Gateway (OCELOT)
                builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
                {
                    var baseUrl = builder.Configuration["OCELOT:BASEURL"];
                    client.BaseAddress = new Uri(baseUrl);
                    Log.Information("OCELOT BaseURL: {BaseUrl}", baseUrl);
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

                // DI Services
                builder.Services.AddScoped<IStoreService, StoreServices>();
                builder.Services.AddScoped<IAuthService, AuthService>();
                builder.Services.AddScoped<IProductService, ProductService>();

                // Session
                builder.Services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(30);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                });

                // AuthZ
                builder.Services.AddAuthorization();

                // IWebHostEnvironment
                builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment);

                var app = builder.Build();

                // Pipeline
                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Home/Error");
                    app.UseHsts();
                }

                //app.UseHttpsRedirection();

                // Log mỗi HTTP request (status, timing, path, v.v.)
                app.UseSerilogRequestLogging();

                app.UseStaticFiles();

                // Map thêm static file từ ổ đĩa nếu có env
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

                app.MapGet("/", context =>
                {
                    context.Response.Redirect("/authentication/login");
                    return Task.CompletedTask;
                });

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Authentication}/{action=Login}/{id?}");

                Log.Information("Adminstrator is up. Listening...");
                app.Run();
            }
            catch (Exception ex)
            {
                // Ghi log mức Fatal khi app khởi động thất bại
                Log.Fatal(ex, "Adminstrator start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
