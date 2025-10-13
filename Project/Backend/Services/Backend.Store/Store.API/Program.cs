using API.Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Store.BLL.Services;
using Store.Common.Configs;
using Store.DAL.Databases;
using Store.DAL.Repository;
using System.Security.Claims;
using System.Text;

namespace Store
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            Console.WriteLine($"ASPNETCORE_ENVIRONMENT = {env}");
            DotNetEnv.Env.Load($".env.{env.ToLower()}");

            var builder = WebApplication.CreateBuilder(args);

            // ==========================
            // 🧱 Cấu hình Serilog
            // ==========================
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog(); // Gắn Serilog thay cho logger mặc định

            try
            {
                Log.Information("🚀 Starting Store API...");

                builder.Configuration.AddEnvironmentVariables();

                // ==========================
                // 🔐 JWT Authentication
                // ==========================
                var jwt = builder.Configuration.GetSection("Jwt");
                var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

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
                                Log.Warning("❌ JWT validation failed: {Message}", context.Exception.Message);
                                return Task.CompletedTask;
                            },
                            OnTokenValidated = context =>
                            {
                                Log.Debug("✅ JWT validated for user: {User}", context.Principal?.Identity?.Name);
                                return Task.CompletedTask;
                            }
                        };
                    });

                builder.Services.AddAuthorization();

                // ==========================
                // 🧩 Static file config
                // ==========================
                builder.Services.Configure<StaticFileConfig>(
                    builder.Configuration.GetSection("StaticFiles"));

                // ==========================
                // 🧠 Dependency Injection
                // ==========================
                builder.Services.AddAutoMapper(cfg => { }, typeof(StoreProfile));
                builder.Services.AddScoped<IStoreRepository, StoreRepository>();
                builder.Services.AddScoped<IStoreService, StoreService>();

                builder.Services.AddDbContext<StoreDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

                // ==========================
                // 🧾 Swagger setup
                // ==========================
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebService.Store", Version = "v1" });

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

                var app = builder.Build();

                // ==========================
                // 🌐 Middleware pipeline
                // ==========================
                if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                // Static files
                var staticFileConfig = app.Configuration
                    .GetSection("StaticFiles:ImageUrl")
                    .Get<ImageUrlConfig>();

                if (staticFileConfig != null && Directory.Exists(staticFileConfig.PhysicalPath))
                {
                    app.UseStaticFiles(new StaticFileOptions
                    {
                        FileProvider = new PhysicalFileProvider(staticFileConfig.PhysicalPath),
                        RequestPath = staticFileConfig.RequestPath
                    });
                    Log.Information("🖼️ Serving static files from {Path}", staticFileConfig.PhysicalPath);
                }
                else
                {
                    Log.Warning("⚠️ Static file path not found or misconfigured.");
                }

                app.UseHttpsRedirection();
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "❌ Store API terminated unexpectedly");
            }
            finally
            {
                Log.Information("🧹 Shutting down Store API...");
                Log.CloseAndFlush();
            }
        }
    }
}
