using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Order.BLL.Services.Interfaces;
using Order.API.Mapping;
using Order.BLL.External;
using Order.BLL.External.Interfaces;
using Order.BLL.Services;
using Order.Common.Configs;
using Order.Common.Urls.Auth;
using Order.Common.Urls.Product;
using Order.Common.Urls.Store;
using Order.DAL.Databases;
using Order.DAL.Repositories;
using Order.DAL.Repository.Interfaces;
using Order.DAL.UnitOfWork.Interfaces;
using Order.Helpers;
using Serilog;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Order.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // =====================================================
            // 🌍 Load environment
            // =====================================================
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            DotNetEnv.Env.Load($".env.{env.ToLower()}");
            Console.WriteLine($"ASPNETCORE_ENVIRONMENT = {env}");

            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddEnvironmentVariables();

            // =====================================================
            // 🧾 Configure Serilog (read from appsettings.json)
            // =====================================================
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();

            try
            {
                Log.Information("🚀 Starting Order API...");

                builder.Services.AddHttpContextAccessor();

                // =====================================================
                // 🔐 JWT Authentication
                // =====================================================
                var jwt = builder.Configuration.GetSection("Jwt");
                var key = Encoding.UTF8.GetBytes(jwt["Key"] ?? throw new InvalidOperationException("JWT Key is missing"));

                builder.Services
                    .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(options =>
                    {
                        options.RequireHttpsMetadata = true;
                        options.SaveToken = true;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = jwt["Issuer"],
                            ValidAudience = jwt["Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(key),
                            ClockSkew = TimeSpan.Zero,
                            RoleClaimType = ClaimTypes.Role
                        };
                        options.Events = new JwtBearerEvents
                        {
                            OnAuthenticationFailed = context =>
                            {
                                Log.Warning("❌ Authentication failed: {Message}", context.Exception.Message);
                                return Task.CompletedTask;
                            },
                            OnChallenge = context =>
                            {
                                Log.Warning("⚠️ JWT Challenge: {Error}", context.ErrorDescription);
                                return Task.CompletedTask;
                            }
                        };
                    });

                builder.Services.AddAuthorization();

                // =====================================================
                // 🧠 AutoMapper
                // =====================================================
                builder.Services.AddAutoMapper(cfg => { }, typeof(OrderProfile));

                // =====================================================
                // 💾 Database
                // =====================================================
                builder.Services.AddDbContext<OrderDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

                // =====================================================
                // 📦 Dependency Injection
                // =====================================================
                builder.Services.AddScoped<HeaderHandler>();
                builder.Services.AddScoped<ICartRepository, CartRepository>();
                builder.Services.AddScoped<IOrderRepository, OrderRepository>();
                builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
                builder.Services.AddScoped<IOrderService, OrderService>();
                builder.Services.AddScoped<ICartService, CartService>();
                builder.Services.AddScoped<IShippingRepository, ShippingRepository>();

                // =====================================================
                // 🌐 HTTP Clients
                // =====================================================
                builder.Services.Configure<AuthEndpoints>(
                    builder.Configuration.GetSection("ServiceUrls:Auth:Endpoints"));

                builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
                {
                    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Auth:BaseUrl"]);
                    client.DefaultRequestHeaders.Add("Auth-Agent", "OrderService/1.0");
                });

                builder.Services.Configure<ProductEndpoints>(
                    builder.Configuration.GetSection("ServiceUrls:Product:Endpoints"));

                builder.Services.AddHttpClient<IProductApiClient, ProductApiClient>(client =>
                {
                    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Product:BaseUrl"]);
                    client.DefaultRequestHeaders.Add("Product-Agent", "OrderService/1.0");
                }).AddHttpMessageHandler<HeaderHandler>();

                builder.Services.Configure<StoreEndpoints>(
                    builder.Configuration.GetSection("ServiceUrls:Store:Endpoints"));

                builder.Services.AddHttpClient<IStoreApiClient, StoreApiClient>(client =>
                {
                    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Store:BaseUrl"]);
                    client.DefaultRequestHeaders.Add("Store-Agent", "OrderService/1.0");
                }).AddHttpMessageHandler<HeaderHandler>();

                // =====================================================
                // ⚙️ Static Files
                // =====================================================
                builder.Services.Configure<StaticFileConfig>(
                    builder.Configuration.GetSection("StaticFiles"));

                // =====================================================
                // 🧩 Controllers
                // =====================================================
                builder.Services.AddControllers()
                    .AddJsonOptions(opt =>
                    {
                        opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    });

                // =====================================================
                // 📘 Swagger
                // =====================================================
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebService.OrderService", Version = "v1" });

                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Description = "Input Token: Bearer {token}"
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                            },
                            Array.Empty<string>()
                        }
                    });
                });

                // =====================================================
                // 🚀 Build app
                // =====================================================
                var app = builder.Build();

                // =====================================================
                // 🧱 Middleware pipeline
                // =====================================================
                if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();

                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();

                Log.Information("✅ Order API started successfully");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "❌ Order API terminated unexpectedly");
            }
            finally
            {
                Log.Information("🧹 Shutting down Order API...");
                Log.CloseAndFlush();
            }
        }
    }
}
