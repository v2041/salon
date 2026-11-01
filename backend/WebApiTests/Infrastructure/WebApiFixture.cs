using Testcontainers.PostgreSql;

namespace WebApiTests.Infrastructure;

public sealed class WebApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:16-alpine")
        .WithDatabase("salon_tests")
        .WithUsername("salon")
        .WithPassword("salon")
        .Build();

    private WebApiFactory _factory = null!;

    public WebApiFactory Factory => _factory;

    public async Task InitializeAsync()
    {
        await _database.StartAsync();
        _factory = new WebApiFactory(_database.GetConnectionString());
        await _factory.ResetDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _database.DisposeAsync();
    }

    public Task ResetDatabaseAsync() => _factory.ResetDatabaseAsync();
}
