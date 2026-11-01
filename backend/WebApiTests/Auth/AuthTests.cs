using System.Net;
using System.Net.Http.Json;
using Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WebApiTests.Infrastructure;

namespace WebApiTests.Auth;

[Collection(WebApiCollection.Name)]
public class AuthTests(WebApiFixture fixture) : HttpTestBase(fixture)
{
    [Fact]
    public async Task Resend_InvalidatesOldCode()
    {
        const string phone = "+79990000020";
        await SeedUserAsync(phone);
        await SendCodeAsync(Client, phone);
        var first = await GetCodeAsync(phone);
        await SendCodeAsync(Client, phone);
        var second = await GetCodeAsync(phone);

        var oldResponse = await Client.PostAsJsonAsync("/api/auth/login", new { phone, code = first });
        Assert.Equal(HttpStatusCode.BadRequest, oldResponse.StatusCode);

        var newResponse = await Client.PostAsJsonAsync("/api/auth/login", new { phone, code = second });
        Assert.Equal(HttpStatusCode.OK, newResponse.StatusCode);
    }

    [Fact]
    public async Task SendCode_SixthWithinHour_Returns429ProblemJson()
    {
        const string phone = "+79990000021";
        for (var i = 0; i < 5; i++)
        {
            var ok = await SendCodeAsync(Client, phone);
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        }

        var limited = await SendCodeAsync(Client, phone);

        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.Equal("application/problem+json", limited.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Login_TwentyFirstAttemptWithinHour_Returns429()
    {
        const string phone = "+79990000022";
        await SeedUserAsync(phone);
        await SendCodeAsync(Client, phone);

        for (var i = 0; i < 20; i++)
        {
            var response = await Client.PostAsJsonAsync("/api/auth/login", new { phone, code = "0000" });
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        var limited = await Client.PostAsJsonAsync("/api/auth/login", new { phone, code = "0000" });
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
    }

    [Fact]
    public async Task SendCode_InDevelopment_ReturnsCode()
    {
        const string phone = "+79990000023";
        using var factory = Factory.WithWebHostBuilder(builder => builder.UseEnvironment("Development"));
        using var client = factory.CreateClient();

        var response = await SendCodeAsync(client, phone);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<SendCodeDto>();
        Assert.False(string.IsNullOrWhiteSpace(body!.Code));
    }

    [Fact]
    public async Task SendCode_InTesting_DoesNotReturnCode()
    {
        const string phone = "+79990000025";

        var response = await SendCodeAsync(Client, phone);

        var body = await response.Content.ReadFromJsonAsync<SendCodeDto>();
        Assert.Null(body!.Code);
    }

    [Fact]
    public async Task Register_WithAdminPhone_PromotesToAdmin()
    {
        const string phone = "+79990000024";
        using var factory = CreateFactoryWithAdminPhone(phone);
        using var client = factory.CreateClient();

        var send = await SendCodeAsync(client, phone);
        Assert.Equal(HttpStatusCode.OK, send.StatusCode);
        var code = await GetCodeAsync(phone);

        var register = await client.PostAsJsonAsync("/api/auth/register", new
        {
            code,
            phone,
            firstName = "Админ",
            lastName = "Тест"
        });

        Assert.Equal(HttpStatusCode.OK, register.StatusCode);
        var role = await WithDbAsync(async db =>
            (await db.Users.AsNoTracking().FirstAsync(u => u.Phone == phone)).Role);
        Assert.Equal(UserRole.Admin, role);
    }

    private WebApplicationFactory<Program> CreateFactoryWithAdminPhone(string phone)
        => Factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AdminPhone"] = phone
                })));

    private static Task<HttpResponseMessage> SendCodeAsync(HttpClient client, string phone)
        => client.PostAsJsonAsync("/api/auth/send-code", new { phone });

    private Task<string> GetCodeAsync(string phone)
        => WithDbAsync(async db =>
            (await db.VerificationCodes.AsNoTracking().FirstAsync(x => x.Phone == phone)).Code);

    private sealed record SendCodeDto(DateTime ExpiresAt, bool IsRegistered, string? Code);
}
