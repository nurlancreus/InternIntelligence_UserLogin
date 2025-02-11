using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using InternIntelligence_UserLogin.Core.Data.Entities;

namespace InternIntelligence_UserLogin.Context
{
    public class CustomSaveChangesInterceptor : SaveChangesInterceptor
    {
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var dbContext = eventData.Context;

            if (dbContext is not null)
            {
                UpdateAuditableEntities(dbContext);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        static void UpdateAuditableEntities(DbContext dbContext)
        {
            DateTime utcNow = DateTime.UtcNow;
            var entities = dbContext.ChangeTracker.Entries<ApplicationUser>();

            foreach (EntityEntry<ApplicationUser> entry in entities)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property(nameof(ApplicationUser.CreatedAt)).CurrentValue = utcNow;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Property(nameof(ApplicationUser.UpdatedAt)).CurrentValue = utcNow;
                }
            }
        }

    }
}
