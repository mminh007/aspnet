
using API.BackgroundServices;
using API.Mapping;
using BLL.Services;
using BLL.Services.Interfaces;
using Common.IntegrationEvents.Events;
using Common.IntegrationEvents.Handlers;
using DAL.Databases;
using DAL.Repository;
using DAL.Repository.Interfaces;
using EventBus.Interfaces;
using EventBusRabbitMQ;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using System.Security.Claims;
using System.Text;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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

            builder.Services.AddDbContext<ProductDbContext>(option =>
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

            builder.Services.AddAutoMapper(cfg => { }, typeof(ProductProfile));

            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductService, ProductService>();


            builder.Services.AddDbContext<ProductDbContext>(option =>
                option.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

            //// EventBus
            //builder.Services.Configure<RabbitMQSettings>(
            //    builder.Configuration.GetSection("RabbitMQ"));

            //// Register EventBus
            //builder.Services.AddSingleton<IEventBus>(sp =>
            //{
            //    var config = sp.GetRequiredService<IOptions<RabbitMQSettings>>().Value;

            //    var factory = new ConnectionFactory
            //    {
            //        HostName = config.HostName,
            //        UserName = config.UserName,
            //        Password = config.Password,
            //        Port = config.Port,
            //        VirtualHost = config.VirtualHost
            //    };

            //    return new RabbitMQEventBus(factory, sp);
            //});

            //builder.Services.AddScoped<IIntegrationEventLogRepository, IntegrationEventLogRepository>();
            //builder.Services.AddScoped<IIntegrationEventService, IntegrationEventService>();

            //// Hosted Service to process outbox messages
            //builder.Services.AddHostedService<OutboxPublisherService>();

            //// Handlers
            //builder.Services.AddTransient<ProductPriceChangedIntegrationEventHandler>();
            //builder.Services.AddTransient<ProductQuantityZeroIntegrationEventHandler>();

            var app = builder.Build();

            //var eventBus = app.Services.GetRequiredService<IEventBus>();

            //// Subscriptions
            //eventBus.Subscribe<ProductPriceChangedIntegrationEvent, ProductPriceChangedIntegrationEventHandler>(
            //    "product_exchange", "product-service-queue", "product.price.changed");

            //eventBus.Subscribe<ProductQuantityZeroIntegrationEvent, ProductQuantityZeroIntegrationEventHandler>(
            //    "product_exchange", "product-service-queue", "product.quantity.zero");


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
