﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using InternIntelligence_UserLogin.Core.Data.Entities;
using Microsoft.EntityFrameworkCore;
using InternIntelligence_UserLogin.Context;
using InternIntelligence_UserLogin.Core.Options.Email;
using InternIntelligence_UserLogin.Core.Options.Token;

namespace InternIntelligence_UserLogin
{
    public static class Configurations
    {
        public static void RegisterServices(this WebApplicationBuilder builder)
        {
            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            #region Register Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 12;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
            });
            #endregion

            #region Register Auth
            // Configure Authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
             .AddJwtBearer(options =>
             {
                 options.TokenValidationParameters = new TokenValidationParameters()
                 {
                     ValidateAudience = true,
                     ValidAudience = builder.Configuration["Token:Access:Audience"],
                     ValidateIssuer = true,
                     ValidIssuer = builder.Configuration["Token:Access:Issuer"],
                     ValidateLifetime = true,
                     ValidateIssuerSigningKey = true,

                     IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Token:Access:SecurityKey"]!)),

                     LifetimeValidator = (notBefore, expires, securityToken, validationParameters) => expires != null && expires > DateTime.UtcNow,

                     NameClaimType = ClaimTypes.Name,
                     RoleClaimType = ClaimTypes.Role
                 };
             });
            #endregion

            #region Register DbContext
            // Add DbContext Interceptors 
            builder.Services.AddScoped<CustomSaveChangesInterceptor>();

            builder.Services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("Default"), sqlOptions => sqlOptions
                .MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
                .EnableSensitiveDataLogging();

                options.AddInterceptors(sp.GetRequiredService<CustomSaveChangesInterceptor>());
            });
            #endregion

            #region Exception Handling
            // Add Exception Handler
            builder.Services.AddExceptionHandler<CustomExceptionHandler>();

            // Add ProblemDetails services
            builder.Services.AddProblemDetails();
            #endregion

            // Register Options pattern
            #region Register Options
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailConfiguration"));
            builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("Token"));
            #endregion
        }

        public static void AddMiddlewares(this WebApplication app) 
        {
            app.UseExceptionHandler();

            app.UseStatusCodePages(async statusCodeCntx
                    => await TypedResults.Problem(statusCode: statusCodeCntx.HttpContext.Response.StatusCode)
                 .ExecuteAsync(statusCodeCntx.HttpContext));

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();
        }
    }
}
