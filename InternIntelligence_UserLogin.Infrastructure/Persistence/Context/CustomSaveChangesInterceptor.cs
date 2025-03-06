using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using InternIntelligence_UserLogin.Core.Abstractions.Base;

namespace InternIntelligence_UserLogin.Infrastructure.Persistence.Context
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
            var entities = dbContext.ChangeTracker.Entries<IAuditable>();

            foreach (EntityEntry<IAuditable> entry in entities)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Property(nameof(IAuditable.CreatedAt)).CurrentValue = utcNow;
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = utcNow;
                }
            }
        }
    }
}
