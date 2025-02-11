using InternIntelligence_UserLogin.Core.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InternIntelligence_UserLogin.Context
{
    // add-migration init -OutputDir ./Context/Migrations
    public class AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : IdentityDbContext<ApplicationUser, IdentityRole, string>(dbContextOptions)
    {
    }
}
