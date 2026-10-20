using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Seeder;

var connection = Environment.GetEnvironmentVariable("ConnectionStrings__SalonDbContext");
if (string.IsNullOrWhiteSpace(connection))
{
    Console.Error.WriteLine("Задайте ConnectionStrings__SalonDbContext.");
    return 1;
}

if (!string.Equals(Environment.GetEnvironmentVariable("ALLOW_DEMO_SEED"), "true", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine("Задайте ALLOW_DEMO_SEED=true.");
    return 1;
}

var reset = args.Contains("--reset", StringComparer.OrdinalIgnoreCase);
var options = new DbContextOptionsBuilder<SalonDbContext>()
    .UseNpgsql(connection)
    .Options;
await using var db = new SalonDbContext(options);
try
{
    await DemoSeeder.SeedAsync(db, reset, CancellationToken.None);
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Ошибка сида: {exception}");
    return 1;
}
