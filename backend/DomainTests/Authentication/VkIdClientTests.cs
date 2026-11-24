using System.Net;
using DomainTests.Notifications;
using Infrastructure.Authentication;

namespace DomainTests.Authentication;

public class VkIdClientTests
{
    private const string AppId = "54917696";
    private const string ServiceToken = "service-secret";

    private const string TokenJson = """
    {
      "access_token": "vk-access-token",
      "refresh_token": "vk-refresh-token",
      "expires_in": 3600,
      "user_id": 1234567890,
      "scope": "phone"
    }
    """;

    private const string UserJson = """
    {
      "user": {
        "user_id": "1234567890",
        "first_name": "Анна",
        "last_name": "Иванова",
        "phone": "79991234567"
      }
    }
    """;

    private static VkIdClient CreateClient(HttpMessageHandler handler, string? serviceToken = ServiceToken)
        => new(new HttpClient(handler), new VkIdOptions { AppId = AppId, ServiceToken = serviceToken });

    private static HttpResponseMessage Json(string content) =>
        new(HttpStatusCode.OK) { Content = new StringContent(content) };

    [Fact]
    public async Task ExchangeCodeAsync_WithCodeAndDeviceId_SendsAuthRequestAndParsesUser()
    {
        var handler = new FakeHttpMessageHandler(request =>
            request.RequestUri!.AbsolutePath == "/oauth2/auth" ? Json(TokenJson) : Json(UserJson));
        var client = CreateClient(handler);

        var user = await client.ExchangeCodeAsync("code-1", "device-1", "verifier-1", "https://salon.test");

        Assert.Equal(2, handler.CallCount);
        Assert.Equal("https://id.vk.ru/oauth2/auth", handler.Uris[0].ToString());
        Assert.Equal("https://id.vk.ru/oauth2/user_info", handler.Uris[1].ToString());
        Assert.Contains("grant_type=authorization_code", handler.Bodies[0]!);
        Assert.Contains("code=code-1", handler.Bodies[0]!);
        Assert.Contains("code_verifier=verifier-1", handler.Bodies[0]!);
        Assert.Contains($"client_id={AppId}", handler.Bodies[0]!);
        Assert.Contains("device_id=device-1", handler.Bodies[0]!);
        Assert.Contains("service_token=service-secret", handler.Bodies[0]!);
        Assert.Contains("redirect_uri=https%3A%2F%2Fsalon.test", handler.Bodies[0]!);
        Assert.Contains("access_token=vk-access-token", handler.Bodies[1]!);
        Assert.Equal("1234567890", user.Id);
        Assert.Equal("Анна", user.FirstName);
        Assert.Equal("Иванова", user.LastName);
        Assert.Equal("79991234567", user.Phone);
    }

    [Fact]
    public async Task ExchangeCodeAsync_WithoutServiceToken_DoesNotSendServiceToken()
    {
        var handler = new FakeHttpMessageHandler(request =>
            request.RequestUri!.AbsolutePath == "/oauth2/auth" ? Json(TokenJson) : Json(UserJson));
        var client = CreateClient(handler, serviceToken: null);

        await client.ExchangeCodeAsync("code-1", "device-1", "verifier-1", "https://salon.test");

        Assert.DoesNotContain("service_token", handler.Bodies[0]!);
    }

    [Fact]
    public async Task ExchangeCodeAsync_WithoutPhone_ReturnsNullPhone()
    {
        const string userJson = """
        {
          "user": { "user_id": "1234567890", "first_name": "Анна" }
        }
        """;
        var handler = new FakeHttpMessageHandler(request =>
            request.RequestUri!.AbsolutePath == "/oauth2/auth" ? Json(TokenJson) : Json(userJson));
        var client = CreateClient(handler);

        var user = await client.ExchangeCodeAsync("code-1", "device-1", "verifier-1", "https://salon.test");

        Assert.Null(user.Phone);
    }

    [Fact]
    public async Task ExchangeCodeAsync_WhenVkReturnsError_ThrowsWithDescription()
    {
        const string content = """
        { "error": "invalid_grant", "error_description": "Code is invalid" }
        """;
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(content)
        });
        var client = CreateClient(handler);

        var exception = await Assert.ThrowsAsync<VkIdException>(
            () => client.ExchangeCodeAsync("code-1", "device-1", "verifier-1", "https://salon.test"));

        Assert.Contains("Code is invalid", exception.Message);
    }

    [Fact]
    public async Task ExchangeCodeAsync_WhenResponseIsNotJson_ThrowsVkIdException()
    {
        var handler = FakeHttpMessageHandler.ReturnsOk("not-json");
        var client = CreateClient(handler);

        await Assert.ThrowsAsync<VkIdException>(
            () => client.ExchangeCodeAsync("code-1", "device-1", "verifier-1", "https://salon.test"));
    }
}
