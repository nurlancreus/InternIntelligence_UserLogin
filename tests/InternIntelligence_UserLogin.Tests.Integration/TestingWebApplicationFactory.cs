using InternIntelligence_UserLogin.API;
using InternIntelligence_UserLogin.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace InternIntelligence_UserLogin.Tests.Integration
{
    public class TestingWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                configBuilder.AddUserSecrets<TestingWebApplicationFactory>();
            });

            builder.UseEnvironment("Testing"); // Ensure tests use Test config

            builder.ConfigureServices((context, services) =>
            {
                var config = context.Configuration;
                var connectionString = config["ConnectionStrings:Default"] ?? "UseInMemoryDB"; // Ensure DB is from User Secrets

                // Remove any existing DbContextOptions registrations
                var contextOptionsDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

                if (contextOptionsDescriptor != null)
                {
                    services.Remove(contextOptionsDescriptor);
                }

                // Remove any existing AppDbContext registrations
                var contextDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(AppDbContext));

                if (contextDescriptor != null)
                {
                    services.Remove(contextDescriptor);
                }

                // Remove ALL registered database providers
                var dbContextDescriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions))
                    .ToList();

                foreach (var descriptor in dbContextDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Register In-Memory Database
                services.AddDbContext<AppDbContext>((sp, options) =>
                {
                    options.UseInMemoryDatabase(connectionString);

                    options.AddInterceptors(sp.GetRequiredService<CustomSaveChangesInterceptor>());
                });
            });
        }
    }
}
