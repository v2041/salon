using Infrastructure.BackgroundServices;
using Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebApiTests.Infrastructure;

public sealed class WebApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    private const string JwtKey = "salon-tests-signing-key-at-least-32-chars";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:SalonDbContext"] = connectionString,
                ["Jwt:Issuer"] = "salon-tests",
                ["Jwt:Audience"] = "salon-tests",
                ["Jwt:Key"] = JwtKey,
                ["Jwt:ExpiryMinutes"] = "60"
            });
        });
        builder.ConfigureTestServices(services =>
        {
            var scheduleServiceDescriptors = services
                .Where(descriptor => descriptor.ServiceType == typeof(IHostedService)
                    && descriptor.ImplementationType == typeof(ScheduleService))
                .ToList();
            foreach (var descriptor in scheduleServiceDescriptors)
            {
                services.Remove(descriptor);
            }
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<SalonDbContext>().Database.Migrate();
        return host;
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SalonDbContext>();
        var tables = db.Model.GetEntityTypes()
            .Select(entity => entity.GetTableName())
            .OfType<string>()
            .Distinct()
            .Select(name => $"\"{name}\"");
        var sql = $"TRUNCATE TABLE {string.Join(", ", tables)} RESTART IDENTITY CASCADE";
        await db.Database.ExecuteSqlRawAsync(sql);
    }
}
