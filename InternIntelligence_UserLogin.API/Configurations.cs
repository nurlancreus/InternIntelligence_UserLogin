using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using InternIntelligence_UserLogin.Core.Options.Email;
using InternIntelligence_UserLogin.Core.Options.Token;
using InternIntelligence_UserLogin.Infrastructure.Persistence.Services;
using InternIntelligence_UserLogin.Infrastructure.Services;
using InternIntelligence_UserLogin.Infrastructure.Services.Mail;
using InternIntelligence_UserLogin.Infrastructure.Persistence.Context;
using InternIntelligence_UserLogin.Core.Entities;
using InternIntelligence_UserLogin.Core.Abstractions.Session;
using InternIntelligence_UserLogin.Infrastructure.Services.Session;
using InternIntelligence_UserLogin.Core.Abstractions.Services;
using InternIntelligence_UserLogin.Core.Abstractions.Services.Mail;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.VisualBasic;

namespace InternIntelligence_UserLogin.API
{
    public static class Configurations
    {
        public static void RegisterServices(this WebApplicationBuilder builder)
        {
            #region Configure Environments
            // Load configurations based on environment
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<Program>(); // Load User Secrets in ALL environments

            #endregion
            // Add services to the container.
            builder.Services.AddAuthorization();

            // Add HttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            #region Register CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(ApiConstants.CorsPolicies.AllowAllPolicy, builder =>
                {
                    builder.AllowAnyOrigin();
                });
            });
            #endregion

            #region Register Rate Limiter
            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddFixedWindowLimiter(policyName: "fixed", limiterOptions =>
             {
                 limiterOptions.PermitLimit = 100;
                 limiterOptions.Window = TimeSpan.FromMinutes(1);
                 limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                 limiterOptions.QueueLimit = 5;
             });
            });

            #endregion

            #region Register Identity
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
                    .AddEntityFrameworkStores<AppDbContext>()
                    .AddDefaultTokenProviders();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 12;
                options.Password.RequiredUniqueChars = 1;

                // SignIn settings
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
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

            #region Add AuthorizationBuilder
            builder.Services.AddAuthorizationBuilder()
            .AddPolicy(ApiConstants.AuthPolicies.AdminsPolicy, policy => policy.RequireAuthenticatedUser().RequireRole("Admin", "SuperAdmin"))
            .AddPolicy(ApiConstants.AuthPolicies.SuperAdminPolicy, policy => policy.RequireAuthenticatedUser().RequireRole("SuperAdmin"))
            .AddPolicy(ApiConstants.AuthPolicies.UserOrAdminPolicy, policy =>
              policy.RequireAuthenticatedUser().RequireAssertion(context =>
             {
                var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var isAdmin = context.User.IsInRole("Admin") || context.User.IsInRole("SuperAdmin");
                if (context.Resource is not HttpContext httpContext) return false;

                var routeData = httpContext.GetRouteData();
                var routeUserId = routeData?.Values["id"]?.ToString();

                routeUserId ??= httpContext.Request.Query["userId"].FirstOrDefault();

                return userIdClaim != null && (routeUserId == userIdClaim || isAdmin);
              }))
            .AddPolicy(ApiConstants.AuthPolicies.UserPolicy, policy =>
              policy.RequireAuthenticatedUser().RequireAssertion(context =>
              {
                var userIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (context.Resource is not HttpContext httpContext) return false;

                var routeData = httpContext.GetRouteData();
                var routeUserId = routeData?.Values["id"]?.ToString();

                routeUserId ??= httpContext.Request.Query["userId"].FirstOrDefault();

                return userIdClaim != null && routeUserId == userIdClaim;
              }));


            #endregion

            #region Register DbContext
            // Add DbContext Interceptors 
            builder.Services.AddScoped<CustomSaveChangesInterceptor>();

            if (!builder.Environment.IsEnvironment("Testing"))
            {
                builder.Services.AddDbContext<AppDbContext>((sp, options) =>
                {
                    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"), sqlOptions => sqlOptions
                    .MigrationsAssembly(typeof(AppDbContext).Assembly.FullName))
                    .EnableSensitiveDataLogging();

                    options.AddInterceptors(sp.GetRequiredService<CustomSaveChangesInterceptor>());
                });
            }

            #endregion

            #region Exception Handling
            // Add ProblemDetails services
            builder.Services.AddProblemDetails();

            // Add Exception Handler
            builder.Services.AddExceptionHandler<CustomExceptionHandler>();
            #endregion

            #region Register DI Services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRoleService, RoleService>();

            builder.Services.AddScoped<IJwtSession, JwtSession>();
            builder.Services.AddScoped<ITokenService, TokenService>();

            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
            builder.Services.AddScoped<IUserEmailService, UserEmailService>();
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

            app.UseStatusCodePages();

            app.UseCors(ApiConstants.CorsPolicies.AllowAllPolicy);

            app.UseRateLimiter();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHttpsRedirection();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }
        }
    }
}
