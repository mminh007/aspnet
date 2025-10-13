
using User.BLL.External;
using User.BLL.Services;
using User.DAL.Databases;
using User.DAL.Repository;
using User.DAL.Repository.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using User.BLL.External.Interfaces;
using User.Common.Urls.Order;
using User.Helpers;
using Serilog;

namespace User
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            DotNetEnv.Env.Load($".env.{env.ToLower()}");

            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddEnvironmentVariables();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();

            // Add services to the container.

            try
            {
                Log.Information("🚀 Starting User API...");

                // ----------------------------
                // Core Services
                // ----------------------------
                builder.Services.AddHttpContextAccessor();

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebService.User", Version = "v1" });

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
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
                });

                // ----------------------------
                // Dependency Injection
                // ----------------------------
                builder.Services.AddScoped<HeaderHandler>();
                builder.Services.AddScoped<IUserRepository, UserRepository>();
                builder.Services.AddScoped<IUserService, UserService>();

                builder.Services.AddDbContext<UserDbContext>(option =>
                    option.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

                // ----------------------------
                // JWT Configuration
                // ----------------------------
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
                                Log.Warning("❌ JWT Authentication failed: {Error}", context.Exception.Message);
                                return Task.CompletedTask;
                            },
                            OnMessageReceived = ctx =>
                            {
                                Log.Debug("🔎 Authorization header received: {Header}", ctx.Request.Headers["Authorization"].ToString());
                                return Task.CompletedTask;
                            }
                        };
                    });

                // ----------------------------
                // HttpClient configuration
                // ----------------------------
                builder.Services.AddHttpClient<IStoreApiClient, StoreApiClient>(client =>
                {
                    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Store:BaseUrl"]);
                    client.DefaultRequestHeaders.Add("Store-Agent", "UserService/1.0");
                });

                builder.Services.AddHttpClient<IOrderApiClient, OrderApiClient>(client =>
                {
                    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Order:BaseUrl"]);
                }).AddHttpMessageHandler<HeaderHandler>();

                builder.Services.Configure<OrderEndpoints>(
                    builder.Configuration.GetSection("ServiceUrls:Order:Endpoints"));

                builder.Services.AddAuthorization();

                // ----------------------------
                // Build application
                // ----------------------------
                var app = builder.Build();

                // Log Authorization header for debugging
                app.Use(async (context, next) =>
                {
                    if (context.Request.Headers.ContainsKey("Authorization"))
                        Log.Debug("👉 Incoming Authorization: {Auth}", context.Request.Headers["Authorization"].ToString());
                    else
                        Log.Debug("👉 Incoming Authorization: <none>");
                    await next();
                });

                // ----------------------------
                // Middleware
                // ----------------------------
                if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "❌ User API terminated unexpectedly");
            }
            finally
            {
                Log.Information("🧹 Application shutting down...");
                Log.CloseAndFlush(); // ensure logs written completely
            }
        }
    }
}
