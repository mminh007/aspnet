
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Oder.BLL.Services.Interfaces;
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
using StackExchange.Redis;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Order.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            DotNetEnv.Env.Load($".env.{env.ToLower()}");

            var builder = WebApplication.CreateBuilder(args);
            
            builder.Configuration.AddEnvironmentVariables();

            builder.Services.AddHttpContextAccessor();

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
                            if (context.Exception is SecurityTokenExpiredException)
                            {
                                context.Response.Headers["Token-Expired"] = "true";
                                context.Response.StatusCode = 401;
                                context.Response.ContentType = "application/json";

                                var jsonResponse = System.Text.Json.JsonSerializer.Serialize(new
                                {
                                    message = "Access token has expired"
                                });
                                return context.Response.WriteAsJsonAsync(jsonResponse);
                            }

                            Console.WriteLine("Authentication failed: " + context.Exception.Message);
                            return Task.CompletedTask;
                        },

                        OnChallenge = context =>
                        {
                            Console.WriteLine("Challenge: " + context.ErrorDescription);
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization();

            builder.Services.AddScoped<HeaderHandler>();

            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<ICartService, CartService>();

            // ========== HTTP CLIENTS ==========
            builder.Services.Configure<AuthEndpoints>(
                builder.Configuration.GetSection("ServiceUrls:Auth:Endpoints"));

            builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Auth:BaseUrl"]);
                client.DefaultRequestHeaders.Add("Auth-Agent", "AuthService/1.0");
            });

            builder.Services.Configure<ProductEndpoints>(
                builder.Configuration.GetSection("ServiceUrls:Product:Endpoints"));

            builder.Services.AddHttpClient<IProductApiClient, ProductApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Product:BaseUrl"]);
                client.DefaultRequestHeaders.Add("Product-Agent", "AuthService/1.0");
            }).AddHttpMessageHandler<HeaderHandler>();

            builder.Services.Configure<StoreEndpoints>(
                builder.Configuration.GetSection("ServiceUrls:Store:Endpoints"));

            builder.Services.AddHttpClient<IStoreApiClient, StoreApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Store:BaseUrl"]);
                client.DefaultRequestHeaders.Add("Store-Agent", "AuthService/1.0");
            }).AddHttpMessageHandler<HeaderHandler>();

            builder.Services.AddAutoMapper(cfg => { }, typeof(OrderProfile));

            //builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
            //{
            //    try
            //    {
            //        var configuration = builder.Configuration.GetConnectionString("Redis");
            //        var configurationOptions = ConfigurationOptions.Parse(configuration!);

            //        // Thêm các tùy chọn bổ sung
            //        configurationOptions.AbortOnConnectFail = false;
            //        configurationOptions.ConnectTimeout = 5000;
            //        configurationOptions.ResponseTimeout = 5000;
            //        configurationOptions.ConnectRetry = 3;

            //        return ConnectionMultiplexer.Connect(configurationOptions);
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Redis connection failed: {ex.Message}");
            //        // Hoặc log lỗi và trả về null để xử lý fallback
            //        throw;
            //    }
            //});
            //builder.Services.AddScoped<RedisCartService>();

            // ========== DB ==========
            builder.Services.AddDbContext<OrderDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));


            builder.Services.AddControllers()
            .AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            // ========== Swagger ==========
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

            builder.Services.Configure<StaticFileConfig>(
                builder.Configuration.GetSection("StaticFiles"));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
    }
}
