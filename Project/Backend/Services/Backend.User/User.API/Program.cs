
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


namespace User
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DotNetEnv.Env.Load();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // Add Dependency injection
            // Connect Auth Service
            builder.Services.AddScoped<HeaderHandler>();

            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddDbContext<UserDbContext>(option =>
                     option.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

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
                            Console.WriteLine($"❌ Authentication failed: {context.Exception.Message}");
                            Console.WriteLine($"❌ Exception type: {context.Exception.GetType().Name}");
                            Console.WriteLine("❌ Auth failed: " + context.Exception);
                            
                            //var authHeader = context.Request.Headers["Authorization"].ToString();
                            //if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                            //{
                            //    try
                            //    {
                            //        var token = authHeader.Substring("Bearer ".Length);
                            //        var parts = token.Split('.');
                            //        if (parts.Length == 3)
                            //        {
                            //            // Decode payload
                            //            var payload = parts[1];
                            //            // Add padding if needed
                            //            switch (payload.Length % 4)
                            //            {
                            //                case 2: payload += "=="; break;
                            //                case 3: payload += "="; break;
                            //            }

                            //            var jsonBytes = Convert.FromBase64String(payload);
                            //            var json = Encoding.UTF8.GetString(jsonBytes);
                            //            Console.WriteLine($"📋 Token payload: {json}");

                            //            // Parse để lấy exp và nbf
                            //            var tokenData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                            //            if (tokenData.ContainsKey("exp"))
                            //            {
                            //                var exp = ((JsonElement)tokenData["exp"]).GetInt64();
                            //                var expTime = DateTimeOffset.FromUnixTimeSeconds(exp);
                            //                Console.WriteLine($"🕐 Token expires at: {expTime} UTC");
                            //                Console.WriteLine($"🕐 Time until expiry: {expTime - DateTimeOffset.UtcNow}");
                            //            }
                            //            if (tokenData.ContainsKey("nbf"))
                            //            {
                            //                var nbf = ((JsonElement)tokenData["nbf"]).GetInt64();
                            //                var nbfTime = DateTimeOffset.FromUnixTimeSeconds(nbf);
                            //                Console.WriteLine($"🕐 Token valid from: {nbfTime} UTC");
                            //                Console.WriteLine($"🕐 Time since valid: {DateTimeOffset.UtcNow - nbfTime}");
                            //            }
                            //        }
                            //    }
                            //    catch (Exception ex)
                            //    {
                            //        Console.WriteLine($"❌ Error parsing token: {ex.Message}");
                            //    }
                            //}

                            return Task.CompletedTask;
                        },

                        OnMessageReceived = ctx =>
                        {
                            Console.WriteLine("🔎 Raw Authorization header JwtBearer sees: " + ctx.Request.Headers["Authorization"]);
                            return Task.CompletedTask;
                        },
                    };
                });

            builder.Services.AddHttpClient<IStoreApiClient, StoreApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Store:BaseUrl"]);
                client.DefaultRequestHeaders.Add("Store-Agent", "AuthService/1.0");
            });

            builder.Services.AddHttpClient<IOrderApiClient, OrderApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Order:BaseUrl"]);
            }).AddHttpMessageHandler<HeaderHandler>();

            builder.Services.Configure<OrderEndpoints>(
                builder.Configuration.GetSection("ServiceUrls:Order:Endpoints"));



            builder.Services.AddAuthorization();

            var app = builder.Build();

            app.Use(async (context, next) =>
            {
                if (context.Request.Headers.ContainsKey("Authorization"))
                {
                    Console.WriteLine("👉 Incoming Authorization: " + context.Request.Headers["Authorization"]);
                }
                else
                {
                    Console.WriteLine("👉 Incoming Authorization: <none>");
                }

                await next();
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
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
