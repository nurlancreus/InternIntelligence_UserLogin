using InternIntelligence_UserLogin.Core.Entities;
using InternIntelligence_UserLogin.Infrastructure.Persistence.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace InternIntelligence_UserLogin.Infrastructure.Persistence.Context
{
    // add-migration init -OutputDir Persistence/Context/Migrations
    public class AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(dbContextOptions)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfigurationsFromAssembly(Assembly.GetAssembly(typeof(ApplicationUserConfiguration))!);

            base.OnModelCreating(builder);
        }
    }
}
