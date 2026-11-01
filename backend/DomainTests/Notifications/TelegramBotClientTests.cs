using System.Net;
using System.Text.Json;
using Infrastructure.Notifications;

namespace DomainTests.Notifications;

public class TelegramBotClientTests
{
    private const string Token = "123456:ABC-DEF";
    private const long ChatId = 123456789L;

    private static TelegramBotClient CreateClient(HttpMessageHandler handler)
        => new(new HttpClient(handler), Token);

    public class SendMessageAsync
    {
        [Fact]
        public async Task SendMessageAsync_WithChatIdAndText_PostsToSendMessage()
        {
            var handler = FakeHttpMessageHandler.ReturnsOk();
            var client = CreateClient(handler);

            await client.SendMessageAsync(ChatId, "Код для входа: 1234. Никому не сообщайте.");

            Assert.Equal(HttpMethod.Post, handler.LastMethod);
            Assert.Equal($"https://api.telegram.org/bot{Token}/sendMessage", handler.LastUri!.ToString());
            using var body = JsonDocument.Parse(handler.LastBody!);
            Assert.Equal(ChatId, body.RootElement.GetProperty("chat_id").GetInt64());
            Assert.Equal(
                "Код для входа: 1234. Никому не сообщайте.",
                body.RootElement.GetProperty("text").GetString());
        }

        [Fact]
        public async Task SendMessageAsync_WithReplyMarkup_SendsKeyboard()
        {
            var handler = FakeHttpMessageHandler.ReturnsOk();
            var client = CreateClient(handler);
            var markup = new
            {
                keyboard = new[] { new[] { new { text = "Поделиться номером", request_contact = true } } }
            };

            await client.SendMessageAsync(ChatId, "Текст", markup);

            using var body = JsonDocument.Parse(handler.LastBody!);
            var keyboard = body.RootElement.GetProperty("reply_markup").GetProperty("keyboard");
            Assert.True(keyboard[0][0].GetProperty("request_contact").GetBoolean());
        }

        [Fact]
        public async Task SendMessageAsync_WhenBotApiFails_ThrowsHttpRequestException()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));
            var client = CreateClient(handler);

            await Assert.ThrowsAsync<HttpRequestException>(
                () => client.SendMessageAsync(ChatId, "Текст"));
        }
    }

    public class GetUpdatesAsync
    {
        [Fact]
        public async Task GetUpdatesAsync_WithMessage_ParsesUpdate()
        {
            const string content = """
            {
              "ok": true,
              "result": [
                {
                  "update_id": 10,
                  "message": {
                    "message_id": 1,
                    "from": { "id": 555, "is_bot": false, "first_name": "Анна" },
                    "chat": { "id": 555, "type": "private" },
                    "date": 1730000000,
                    "text": "/start"
                  }
                }
              ]
            }
            """;
            var handler = FakeHttpMessageHandler.ReturnsOk(content);
            var client = CreateClient(handler);

            var updates = await client.GetUpdatesAsync(10);

            Assert.Equal($"https://api.telegram.org/bot{Token}/getUpdates", handler.LastUri!.ToString());
            using var body = JsonDocument.Parse(handler.LastBody!);
            Assert.Equal(50, body.RootElement.GetProperty("timeout").GetInt32());
            Assert.Equal(10, body.RootElement.GetProperty("offset").GetInt64());
            var update = Assert.Single(updates);
            Assert.Equal(10, update.UpdateId);
            Assert.Equal(555, update.Message!.ChatId);
            Assert.Equal(555, update.Message.FromId);
            Assert.Equal("/start", update.Message.Text);
            Assert.Null(update.Message.Contact);
        }

        [Fact]
        public async Task GetUpdatesAsync_WithContact_ParsesPhoneAndUserId()
        {
            const string content = """
            {
              "ok": true,
              "result": [
                {
                  "update_id": 11,
                  "message": {
                    "message_id": 2,
                    "from": { "id": 555, "is_bot": false, "first_name": "Анна" },
                    "chat": { "id": 555, "type": "private" },
                    "date": 1730000001,
                    "contact": { "phone_number": "+79991234567", "first_name": "Анна", "user_id": 555 }
                  }
                }
              ]
            }
            """;
            var handler = FakeHttpMessageHandler.ReturnsOk(content);
            var client = CreateClient(handler);

            var updates = await client.GetUpdatesAsync();

            var contact = Assert.Single(updates).Message!.Contact;
            Assert.Equal("+79991234567", contact!.PhoneNumber);
            Assert.Equal(555, contact.UserId);
        }
    }
}
