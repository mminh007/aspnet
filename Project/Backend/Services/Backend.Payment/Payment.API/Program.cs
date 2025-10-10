
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

            builder.Services.AddAutoMapper(cfg => { }, typeof(PaymentProfile));

            builder.Services.AddDbContext<PaymentDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

            builder.Services.AddControllers()
            .AddJsonOptions(opt =>
            {
                opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
                        new OpenApiSecurityScheme {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // Stripe Config
            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            builder.Services.AddScoped<HeaderHandler>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            builder.Services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Auth:BaseUrl"]);
            });

            builder.Services.Configure<AuthEndpoints>(
                builder.Configuration.GetSection("ServiceUrls:Auth:Endpoints"));

            builder.Services.AddHttpClient<IOrderApiClient, OrderApiClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:Order:BaseUrl"]);
            }).AddHttpMessageHandler<HeaderHandler>();

            builder.Services.Configure<OrderEndpoints>(
                builder.Configuration.GetSection("ServiceUrls:Order:Endpoints"));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
