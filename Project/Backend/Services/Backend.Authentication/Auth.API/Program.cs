using Auth.BLL.External;
using Auth.BLL.Services;
using Auth.BLL.Services.Interfaces;
using Auth.DAL.Databases;
using Auth.DAL.Models.Entities;
using Auth.DAL.Repository.Interfaces;
using Auth.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Security.Claims;
using System.Text;

namespace Auth.API
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
            // 🧾 Configure Serilog
            // =====================================================
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            builder.Host.UseSerilog();

            try
            {
                Log.Information("🚀 Starting Authentication API (Environment: {Env})...", env);

                // =====================================================
                // 🔐 JWT
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
                                if (context.Exception is SecurityTokenExpiredException)
                                {
                                    Log.Warning("⚠️ Token expired for request {Path}", context.Request.Path);
                                    context.Response.Headers["Token-Expired"] = "true";
                                    context.Response.StatusCode = 401;
                                    context.Response.ContentType = "application/json";

                                    var jsonResponse = System.Text.Json.JsonSerializer.Serialize(new
                                    {
                                        message = "Access token has expired"
                                    });
                                    return context.Response.WriteAsJsonAsync(jsonResponse);
                                }

                                Log.Error(context.Exception, "❌ Authentication failed");
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

                // =====================================================
                // 📦 Dependency Injection
                // =====================================================
                builder.Services.AddHttpClient<UserApiService>(client =>
                {
                    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:User:BaseUrl"]);
                    client.DefaultRequestHeaders.Add("User-Agent", "AuthService/1.0");
                });

                builder.Services.AddScoped<IAuthRepository, AuthRepository>();
                builder.Services.AddScoped<ITokenManager, TokenManager>();
                builder.Services.AddScoped<IAuthService, AuthService>();
                builder.Services.AddScoped<IEmailService, EmailService>();
                builder.Services.AddScoped<IPasswordHasher<IdentityModel>, PasswordHasher<IdentityModel>>();

                builder.Services.AddDbContext<AuthDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

                // =====================================================
                // 📘 Swagger
                // =====================================================
                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebService.Authentication", Version = "v1" });

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
                // SSL Configuration
                // =====================================================
                if (builder.Environment.IsDevelopment())
                {
                    Log.Information("🧪 Development mode: Allowing any SSL certificate.");
                    builder.Services.AddHttpClient("", client => { })
                        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback =
                                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                        });
                }
                else if (builder.Environment.IsProduction())
                {
                    Log.Information("🏭 Production mode: Using trusted certificates only.");
                    var trustedThumbprints = builder.Configuration
                        .GetSection("TrustedCertificates:Thumbprints")
                        .Get<string[]>() ?? Array.Empty<string>();

                    builder.Services.AddHttpClient("", client => { })
                        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
                            {
                                if (errors == System.Net.Security.SslPolicyErrors.None)
                                    return true;

                                if (cert != null && trustedThumbprints.Contains(cert.Thumbprint))
                                    return true;

                                Log.Warning("❌ Untrusted certificate: {Thumbprint}", cert?.Thumbprint);
                                return false;
                            }
                        });
                }

                // =====================================================
                // 🚀 Build & Run app
                // =====================================================
                var app = builder.Build();

                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
                });

                if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseAuthentication();
                app.UseAuthorization();
                app.MapControllers();

                Log.Information("✅ Authentication API started successfully on port {Port}", app.Urls);
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "💥 Authentication API terminated unexpectedly");
            }
            finally
            {
                Log.Information("🧹 Shutting down Authentication API...");
                Log.CloseAndFlush();
            }
        }
    }
}
