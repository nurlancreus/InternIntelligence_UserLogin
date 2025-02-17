using InternIntelligence_UserLogin.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace InternIntelligence_UserLogin.Infrastructure.Persistence.Context
{
    public static class AppDbContextDataSeeder
    {
        public static void SeedSuperAdmin(this ModelBuilder builder)
        {
            #region Seed Super Admin
            var superAdminRole = ApplicationRole.Create("SuperAdmin");

            superAdminRole.Id = Guid.Parse("e1a0b1b0-0001-0000-0000-000000000001");
            superAdminRole.NormalizedName = "SUPERADMIN";

            builder.Entity<ApplicationRole>().HasData(superAdminRole);

            // Seed the admin user
            var passwordHasher = new PasswordHasher<ApplicationUser>();

            var superAdminUser = ApplicationUser.Create("Admin", "Super", "adminsuper", "admin@example.com");

            superAdminUser.Id = Guid.Parse("e1a0b1b0-0001-0000-0000-000000000002");
            superAdminUser.NormalizedUserName = "ADMINSUPER";
            superAdminUser.NormalizedEmail = "ADMIN@EXAMPLE.COM";
            superAdminUser.EmailConfirmed = true;
            superAdminUser.SecurityStamp = Guid.NewGuid().ToString();

            superAdminUser.PasswordHash = passwordHasher.HashPassword(superAdminUser, "Ghujtyrtyu456$");

            builder.Entity<ApplicationUser>().HasData(superAdminUser);

            // Seed user role mapping
            builder.Entity<IdentityUserRole<Guid>>().HasData(
                new IdentityUserRole<Guid>
                {
                    UserId = superAdminUser.Id,
                    RoleId = superAdminRole.Id,
                }
            );
            #endregion;
        }
    }
}
