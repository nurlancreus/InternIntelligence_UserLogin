using InternIntelligence_UserLogin.Core.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InternIntelligence_UserLogin.Infrastructure.Persistence.Context
{
    // add-migration init -OutputDir .Persistence/Context/Migrations
    public class AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : IdentityDbContext<ApplicationUser, IdentityRole, string>(dbContextOptions)
    {
    }
}
