using Adminstrator.Helpers;
using Adminstrator.HttpsClients;
using Adminstrator.HttpsClients.Auths;
using Adminstrator.HttpsClients.Interfaces;
using Adminstrator.Middlewares;
using Adminstrator.Services;
using Adminstrator.Services.Interfaces;


namespace Adminstrator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            SettingsHelper.Configure(builder.Configuration);

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<HeaderHandler>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Connect API Product Backend Service
            builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Ocelot:BaseUrl"]);
            });

            builder.Services.AddHttpClient<IStoreApiClient, StoreApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Ocelot:BaseUrl"]);
            }).AddHttpMessageHandler<HeaderHandler>();

            builder.Services.AddHttpClient<IProductApiClient, ProductApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["Ocelot:BaseUrl"]);
            }).AddHttpMessageHandler<HeaderHandler>();

            // Connect Auth Service

            builder.Services.AddScoped<IStoreServices, StoreServices>();
            builder.Services.AddScoped<IAuthServices, AuthServices>();
            builder.Services.AddScoped<IProductService, ProductService>();
            // Connect Service


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
