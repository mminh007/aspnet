using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Payment.API.Mapping;
using Payment.BLL.External;
using Payment.BLL.External.Interface;
using Payment.BLL.Services;
using Payment.BLL.Services.Interfaces;
using Payment.Common.Urls.Auth;
using Payment.Common.Urls.Order;
using Payment.DAL;
using Payment.DAL.Repository;
using Payment.DAL.Repository.Interfaces;
using Payment.DAL.UnitOfWork;
using Payment.DAL.UnitOfWork.Interfaces;
using Payment.Helpers;
using Serilog;
using Stripe;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Payment.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ================================
            // 🌍 Load .env
            // ================================
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            DotNetEnv.Env.Load($".env.{env.ToLower()}");
            Console.WriteLine($"ASPNETCORE_ENVIRONMENT = {env}");

            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddEnvironmentVariables();

            // ================================
            // 🧾 Serilog
            // ================================
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();

            try
            {
                Log.Information("🚀 Starting Payment API...");

                builder.Services.AddHttpContextAccessor();

                // ================================
                // 🔐 JWT Authentication
                // ================================
                var jwt = builder.Configuration.GetSection("Jwt");
                var key = Encoding.UTF8.GetBytes(jwt["Key"] ?? throw new InvalidOperationException("JWT Key missing"));

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
                                Log.Warning("⚠️ JWT Challenge: {ErrorDescription}", context.ErrorDescription);
                                return Task.CompletedTask;
                            }
                        };
                    });

                builder.Services.AddAuthorization();

                // ================================
                // 💳 Stripe Config
                // ================================
                StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

                // ================================
                // 🧠 AutoMapper
                // ================================
                builder.Services.AddAutoMapper(cfg => { }, typeof(PaymentProfile));

                // ================================
                // 💾 Database
                // ================================
                builder.Services.AddDbContext<PaymentDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

                // ================================
                // 🧩 Dependency Injection
                // ================================
                builder.Services.AddScoped<HeaderHandler>();
                builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
                builder.Services.AddScoped<IPaymentService, PaymentService>();
                builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

                builder.Services.AddHttpClient<IStoreApiClient, StoreApiClient>(client =>
                {
                    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Store:BaseUrl"]);
                });

                // Auth API client
                builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
                {
                    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Auth:BaseUrl"]);
                });

                builder.Services.Configure<AuthEndpoints>(
                    builder.Configuration.GetSection("ServiceUrls:Auth:Endpoints"));

                // Order API client
                builder.Services.AddHttpClient<IOrderApiClient, OrderApiClient>(client =>
                {
                    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Order:BaseUrl"]);
                }).AddHttpMessageHandler<HeaderHandler>();

                builder.Services.Configure<OrderEndpoints>(
                    builder.Configuration.GetSection("ServiceUrls:Order:Endpoints"));

                // ================================
                // 🌐 Controllers & JSON options
                // ================================
                builder.Services.AddControllers()
                    .AddJsonOptions(opt =>
                    {
                        opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    });

                // ================================
                // 📘 Swagger
                // ================================
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebService.PaymentService", Version = "v1" });

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
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                            },
                            Array.Empty<string>()
                        }
                    });
                });

                // ================================
                // 🚀 Build app
                // ================================
                var app = builder.Build();

                // ================================
                // 🔧 Middleware pipeline
                // ================================
                if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();

                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();

                Log.Information("✅ Payment API started successfully");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "❌ Payment API terminated unexpectedly");
            }
            finally
            {
                Log.Information("🧹 Shutting down Payment API...");
                Log.CloseAndFlush();
            }
        }
    }
}
