using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebApiTests.Infrastructure;

namespace WebApiTests.Smoke;

[Collection(WebApiCollection.Name)]
public class ProblemDetailsSmokeTests(WebApiFixture fixture) : HttpTestBase(fixture)
{
    [Fact]
    public async Task Get_WithUnknownRoute_ReturnsProblemJson404()
    {
        var response = await Client.GetAsync("/api/unknown-route");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(404, problem.RootElement.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task SendCode_WithInvalidPhone_ReturnsProblemJson400WithErrors()
    {
        var response = await Client.PostAsJsonAsync(
            "/api/auth/send-code",
            new { phone = "123" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(400, problem.RootElement.GetProperty("status").GetInt32());
        Assert.Equal(JsonValueKind.Object, problem.RootElement.GetProperty("errors").ValueKind);
    }
}
