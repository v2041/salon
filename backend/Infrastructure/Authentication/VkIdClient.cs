using System.Text.Json;

namespace Infrastructure.Authentication;

/// <summary>
/// Клиент VK ID: обмен кода подтверждения на данные пользователя.
/// Сбой VK ID — <see cref="VkIdException"/>.
/// </summary>
public class VkIdClient(HttpClient httpClient, VkIdOptions options, ILogger<VkIdClient> logger)
{
    public const string HttpClientName = "VkId";
    public const int HttpClientTimeoutSeconds = 15;

    private const string ApiBaseUrl = "https://id.vk.ru";

    public async Task<VkUserInfo> ExchangeCodeAsync(
        string code,
        string deviceId,
        string codeVerifier,
        string redirectUrl,
        CancellationToken cancellationToken = default)
    {
        using var tokenResponse = await httpClient.PostAsync(
            $"{ApiBaseUrl}/oauth2/auth",
            new FormUrlEncodedContent(BuildAuthPayload(code, deviceId, codeVerifier, redirectUrl)),
            cancellationToken);
        using var tokenDocument = await ReadAsync(tokenResponse, cancellationToken);
        var accessToken = GetStringOrNull(tokenDocument.RootElement, "access_token")
            ?? throw new VkIdException("VK ID не вернул access_token.");

        using var infoResponse = await httpClient.PostAsync(
            $"{ApiBaseUrl}/oauth2/user_info",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["client_id"] = options.AppId!,
                ["access_token"] = accessToken
            }),
            cancellationToken);
        using var infoDocument = await ReadAsync(infoResponse, cancellationToken);
        if (!infoDocument.RootElement.TryGetProperty("user", out var user)
            || user.ValueKind != JsonValueKind.Object)
        {
            throw new VkIdException("VK ID не вернул данные пользователя.");
        }

        var id = user.TryGetProperty("user_id", out var idValue) ? idValue.ToString() : null;
        var rawPhone = user.TryGetProperty("phone", out var phoneEl)
            ? $"{phoneEl.ValueKind}: '{phoneEl}'"
            : "<missing>";
        logger.LogWarning("VK ID user_info phone => {RawPhone}", rawPhone);
        return new VkUserInfo(
            id ?? throw new VkIdException("VK ID не вернул идентификатор пользователя."),
            GetStringOrNull(user, "first_name"),
            GetStringOrNull(user, "last_name"),
            GetStringOrNull(user, "phone"));
    }

    private Dictionary<string, string> BuildAuthPayload(
        string code, string deviceId, string codeVerifier, string redirectUrl)
    {
        var payload = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["code_verifier"] = codeVerifier,
            ["client_id"] = options.AppId!,
            ["device_id"] = deviceId,
            ["redirect_uri"] = redirectUrl
        };
        if (!string.IsNullOrWhiteSpace(options.ServiceToken))
        {
            payload["service_token"] = options.ServiceToken;
        }

        return payload;
    }

    private static async Task<JsonDocument> ReadAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        JsonDocument document;
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        }
        catch (JsonException exception)
        {
            throw new VkIdException("VK ID вернул ответ не в формате JSON.", exception);
        }

        if (response.IsSuccessStatusCode)
            return document;

        var description = GetStringOrNull(document.RootElement, "error_description");
        document.Dispose();
        throw new VkIdException(description ?? $"VK ID вернул ошибку {(int)response.StatusCode}.");
    }

    private static string? GetStringOrNull(JsonElement element, string property) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(property, out var value)
        && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}

/// <summary>Сбой VK ID: ошибка API или ответ не в формате JSON.</summary>
public class VkIdException(string message, Exception? innerException = null)
    : Exception(message, innerException);
