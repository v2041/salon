using System.Net.Http.Json;
using System.Text.Json;

namespace Infrastructure.Notifications;

/// <summary>
/// Тонкая обёртка над Bot API. Сбой Bot API — исключение.
/// <c>SendMessageAsync</c> завершается через 5 секунд,
/// <c>GetUpdatesAsync</c> ждёт до <see cref="LongPollingTimeoutSeconds"/> секунд (long polling).
/// </summary>
public class TelegramBotClient(HttpClient httpClient, string token) : ITelegramMessageSender
{
    public const string HttpClientName = "TelegramBot";
    public const int HttpClientTimeoutSeconds = 60;
    public const int SendMessageTimeoutSeconds = 5;
    public const int LongPollingTimeoutSeconds = 50;

    private const string ApiBaseUrl = "https://api.telegram.org";

    public Task SendMessageAsync(long chatId, string text, CancellationToken cancellationToken = default)
        => SendMessageAsync(chatId, text, null, cancellationToken);

    public async Task SendMessageAsync(
        long chatId,
        string text,
        object? replyMarkup,
        CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, object>
        {
            ["chat_id"] = chatId,
            ["text"] = text
        };
        if (replyMarkup is not null)
        {
            payload["reply_markup"] = replyMarkup;
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(SendMessageTimeoutSeconds));

        using var response = await httpClient.PostAsJsonAsync(
            MethodUrl("sendMessage"), payload, timeout.Token);
        response.EnsureSuccessStatusCode();
        using var document = await ParseJsonAsync(response, "sendMessage", timeout.Token);
        EnsureOk(document.RootElement, "sendMessage");
    }

    public async Task<IReadOnlyList<TelegramUpdate>> GetUpdatesAsync(
        long? offset = null,
        CancellationToken cancellationToken = default)
    {
        var payload = new Dictionary<string, object>
        {
            ["timeout"] = LongPollingTimeoutSeconds
        };
        if (offset is not null)
        {
            payload["offset"] = offset;
        }

        using var response = await httpClient.PostAsJsonAsync(
            MethodUrl("getUpdates"), payload, cancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = await ParseJsonAsync(response, "getUpdates", cancellationToken);
        EnsureOk(document.RootElement, "getUpdates");
        return ParseUpdates(document.RootElement);
    }

    private string MethodUrl(string method) => $"{ApiBaseUrl}/bot{token}/{method}";

    private static async Task<JsonDocument> ParseJsonAsync(
        HttpResponseMessage response,
        string method,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        }
        catch (JsonException exception)
        {
            throw new HttpRequestException(
                $"Bot API вернул ответ не в формате JSON на {method}.", exception);
        }
    }

    private static void EnsureOk(JsonElement root, string method)
    {
        if (root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty("ok", out var ok)
            && ok.ValueKind == JsonValueKind.True)
        {
            return;
        }

        string? description = null;
        if (root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty("description", out var value)
            && value.ValueKind == JsonValueKind.String)
        {
            description = value.GetString();
        }

        throw new HttpRequestException(
            description ?? $"Bot API вернул ошибку на {method}.");
    }

    private static IReadOnlyList<TelegramUpdate> ParseUpdates(JsonElement root)
    {
        var updates = new List<TelegramUpdate>();
        if (!root.TryGetProperty("result", out var result) || result.ValueKind != JsonValueKind.Array)
        {
            return updates;
        }

        foreach (var item in result.EnumerateArray())
        {
            var updateId = item.GetProperty("update_id").GetInt64();
            TelegramMessage? message = null;
            if (item.TryGetProperty("message", out var element) && element.ValueKind == JsonValueKind.Object)
            {
                message = ParseMessage(element);
            }
            updates.Add(new TelegramUpdate(updateId, message));
        }

        return updates;
    }

    private static TelegramMessage ParseMessage(JsonElement element)
    {
        var chatId = element.GetProperty("chat").GetProperty("id").GetInt64();
        long? fromId = null;
        if (element.TryGetProperty("from", out var from) && from.TryGetProperty("id", out var fromIdValue))
        {
            fromId = fromIdValue.GetInt64();
        }

        var text = element.TryGetProperty("text", out var textValue) ? textValue.GetString() : null;
        TelegramContact? contact = null;
        if (element.TryGetProperty("contact", out var contactValue) && contactValue.ValueKind == JsonValueKind.Object)
        {
            long? userId = null;
            if (contactValue.TryGetProperty("user_id", out var userIdValue))
            {
                userId = userIdValue.GetInt64();
            }
            contact = new TelegramContact(contactValue.GetProperty("phone_number").GetString()!, userId);
        }

        return new TelegramMessage(chatId, fromId, text, contact);
    }
}
