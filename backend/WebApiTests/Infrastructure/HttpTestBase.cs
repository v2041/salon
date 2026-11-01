namespace WebApiTests.Infrastructure;

public abstract class HttpTestBase(WebApiFixture fixture) : IAsyncLifetime
{
    protected WebApiFactory Factory => fixture.Factory;

    protected HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await fixture.ResetDatabaseAsync();
        Client = Factory.CreateClient();
    }

    public Task DisposeAsync()
    {
        Client?.Dispose();
        return Task.CompletedTask;
    }
}
