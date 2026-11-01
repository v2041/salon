using System.Net;
using WebApiTests.Infrastructure;

namespace WebApiTests.Health;

[Collection(WebApiCollection.Name)]
public class HealthzTests(WebApiFixture fixture) : HttpTestBase(fixture)
{
    [Fact]
    public async Task Get_WithReachableDatabase_ReturnsOk()
    {
        var response = await Client.GetAsync("/healthz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_WithUnreachableDatabase_ReturnsServiceUnavailableWithoutSecrets()
    {
        const string password = "healthz-secret-password";
        var connectionString =
            $"Host=127.0.0.1;Port=1;Database=salon;Username=salon;Password={password};Timeout=1";
        await using var factory = new WebApiFactory(connectionString, applyMigrations: false);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/healthz");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain(password, body);
        Assert.DoesNotContain("Npgsql", body);
        Assert.DoesNotContain("127.0.0.1", body);
    }
}
