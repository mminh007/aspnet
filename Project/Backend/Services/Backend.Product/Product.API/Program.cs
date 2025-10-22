//using API.BackgroundServices;
using API.Mapping;
using BLL.Services;
using BLL.Services.Interfaces;
//using Common.IntegrationEvents.Events;
//using Common.IntegrationEvents.Handlers;
using DAL.Databases;
using DAL.Repository;
using DAL.Repository.Interfaces;
//using EventBus.Interfaces;
//using EventBusRabbitMQ;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Product.Common.Configs;
//using RabbitMQ.Client;
using Serilog;
using System.Security.Claims;
using System.Text;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // 🧱 Load eNV
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            DotNetEnv.Env.Load($".env.{env.ToLower()}");
            Console.WriteLine($"ASPNETCORE_ENVIRONMENT = {env}");

            var builder = WebApplication.CreateBuilder(args);

            
            builder.Configuration.AddEnvironmentVariables();

            // ==========================
            // 🧾 Serilog
            // ==========================
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog(); 

            try
            {
                Log.Information("🚀 Starting Product API...");

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
                                Log.Warning("❌ JWT Authentication failed: {Message}", context.Exception.Message);
                                return Task.CompletedTask;
                            },
                            OnTokenValidated = context =>
                            {
                                Log.Information("✅ JWT validated for user: {User}", context.Principal?.Identity?.Name);
                                return Task.CompletedTask;
                            }
                        };
                    });

                builder.Services.AddAuthorization();

                // ==========================
                // ⚙️ Database & Repository
                // ==========================
                builder.Services.AddDbContext<ProductDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

                builder.Services.AddScoped<IProductRepository, ProductRepository>();
                builder.Services.AddScoped<IProductService, ProductService>();

                // ==========================
                // 🧩 Static file config
                // ==========================
                builder.Services.Configure<StaticFileConfig>(
                    builder.Configuration.GetSection("StaticFiles"));

                // ==========================
                // 🧠 AutoMapper
                // ==========================
                builder.Services.AddAutoMapper(cfg => { }, typeof(ProductProfile));

                // ==========================
                // 🧱 Swagger
                // ==========================
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebService.Product", Version = "v1" });

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

                // ==========================
                // 🐇 (Optional) RabbitMQ / EventBus
                // ==========================
                // builder.Services.Configure<RabbitMQSettings>(
                //     builder.Configuration.GetSection("RabbitMQ"));
                //
                // builder.Services.AddSingleton<IEventBus>(sp =>
                // {
                //     var config = sp.GetRequiredService<IOptions<RabbitMQSettings>>().Value;
                //
                //     var factory = new ConnectionFactory
                //     {
                //         HostName = config.HostName,
                //         UserName = config.UserName,
                //         Password = config.Password,
                //         Port = config.Port,
                //         VirtualHost = config.VirtualHost
                //     };
                //
                //     return new RabbitMQEventBus(factory, sp);
                // });
                //
                // builder.Services.AddScoped<IIntegrationEventLogRepository, IntegrationEventLogRepository>();
                // builder.Services.AddScoped<IIntegrationEventService, IntegrationEventService>();
                // builder.Services.AddHostedService<OutboxPublisherService>();
                // builder.Services.AddTransient<ProductPriceChangedIntegrationEventHandler>();
                // builder.Services.AddTransient<ProductQuantityZeroIntegrationEventHandler>();

                var app = builder.Build();

                // ==========================
                // 🌐 Configure HTTP Pipeline
                // ==========================
                if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                // Static file config
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
                    Log.Warning("⚠️ Static file path is not configured or does not exist.");
                }

                app.UseHttpsRedirection();
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();

                // ==========================
                // ✅ Run Application
                // ==========================
                Log.Information("✅ Product API started successfully.");
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "❌ Product API terminated unexpectedly");
            }
            finally
            {
                Log.Information("🧹 Shutting down Product API...");
                Log.CloseAndFlush();
            }
        }
    }
}
