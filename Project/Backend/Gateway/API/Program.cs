using API.Middlewares;
using DotNetEnv;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Serilog;
using System.Security.Claims;
using System.Text;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // ==========================================================
            // 🌍 Load environment
            // ==========================================================
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            Env.Load($".env.{env.ToLower()}");

            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddEnvironmentVariables();

            // ==========================================================
            // 🧾 Configure Serilog (đọc từ appsettings.json)
            // ==========================================================
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();

            try
            {
                Log.Information("🚀 Starting API Gateway (Environment: {Env})...", env);

                // ==========================================================
                // 🔧 Load Ocelot configuration
                // ==========================================================
                var ocelotFile = $"ocelotsettings.{env.ToLower()}.json";
                if (!File.Exists(ocelotFile))
                {
                    Log.Warning("⚠️ {File} not found. Using default ocelotsettings.json", ocelotFile);
                    ocelotFile = "ocelotsettings.json";
                }
                else
                {
                    Log.Information("✅ Using {File} for Ocelot configuration", ocelotFile);
                }

                builder.Configuration.AddJsonFile(ocelotFile, optional: false, reloadOnChange: true);

                // ==========================================================
                // 🔐 JWT Authentication
                // ==========================================================
                var jwt = builder.Configuration.GetSection("Jwt");
                var key = Encoding.UTF8.GetBytes(jwt["Key"] ?? throw new InvalidOperationException("JWT key missing"));

                builder.Services.AddAuthentication("Bearer")
                    .AddJwtBearer("Bearer", options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateLifetime = true,
                            ValidateIssuerSigningKey = true,
                            ValidIssuer = jwt["Issuer"],
                            ValidAudience = jwt["Audience"],
                            IssuerSigningKey = new SymmetricSecurityKey(key),
                            RoleClaimType = ClaimTypes.Role,
                        };

                        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                        {
                            OnAuthenticationFailed = context =>
                            {
                                Log.Warning("❌ JWT Authentication failed: {Message}", context.Exception.Message);
                                return Task.CompletedTask;
                            }
                        };
                    });

                builder.Services.AddAuthorization();

                // ==========================================================
                // 📘 Swagger
                // ==========================================================
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API Gateway", Version = "v1" });
                });

                // ==========================================================
                // 🧩 Ocelot setup
                // ==========================================================
                builder.Services.AddOcelot(builder.Configuration);

                // ==========================================================
                // 🔥 Build the app
                // ==========================================================
                var app = builder.Build();

                if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseAuthentication();
                app.UseAuthorization();

                // Custom middleware (optional)
                app.UseTokenRefresh();

                app.MapControllers();

                // ==========================================================
                // 🚦 Run Ocelot Gateway
                // ==========================================================
                await app.UseOcelot();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "💥 API Gateway terminated unexpectedly");
            }
            finally
            {
                Log.Information("🧹 Shutting down API Gateway...");
                Log.CloseAndFlush();
            }
        }
    }
}
