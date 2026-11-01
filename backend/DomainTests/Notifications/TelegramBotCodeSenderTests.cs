using System.Net;
using System.Text.Json;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DomainTests.Notifications;

public class TelegramBotCodeSenderTests
{
    private const string Phone = "+79991234567";
    private const string Code = "1234";
    private const long ChatId = 123456789L;
    private const string Username = "salon_test_bot";

    private static SalonDbContext CreateDb(params TelegramBinding[] bindings)
    {
        var options = new DbContextOptionsBuilder<SalonDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new SalonDbContext(options);
        db.TelegramBindings.AddRange(bindings);
        db.SaveChanges();
        return db;
    }

    private static TelegramBotCodeSender CreateSender(
        SalonDbContext db,
        HttpClient httpClient,
        string? username = Username)
        => new(db, new TelegramBotClient(httpClient, "123456:ABC-DEF"),
            Options.Create(new TelegramOptions { Username = username }));

    public class SendAsync
    {
        [Fact]
        public async Task SendAsync_WithBinding_SendsCodeMessage()
        {
            var handler = FakeHttpMessageHandler.ReturnsOk();
            using var httpClient = new HttpClient(handler);
            using var db = CreateDb(TelegramBinding.Create(Phone, ChatId));
            var sender = CreateSender(db, httpClient);

            var result = await sender.SendAsync(Phone, Code, CancellationToken.None);

            Assert.True(result.Delivered);
            Assert.Null(result.BindingUrl);
            Assert.Equal(1, handler.CallCount);
            using var body = JsonDocument.Parse(handler.LastBody!);
            Assert.Equal(ChatId, body.RootElement.GetProperty("chat_id").GetInt64());
            Assert.Equal(
                "Код для входа: 1234. Никому не сообщайте.",
                body.RootElement.GetProperty("text").GetString());
        }

        [Fact]
        public async Task SendAsync_WithoutBinding_ReturnsBindingUrlAndSkipsBotApi()
        {
            var handler = FakeHttpMessageHandler.ReturnsOk();
            using var httpClient = new HttpClient(handler);
            using var db = CreateDb();
            var sender = CreateSender(db, httpClient);

            var result = await sender.SendAsync(Phone, Code, CancellationToken.None);

            Assert.False(result.Delivered);
            Assert.Equal("https://t.me/salon_test_bot", result.BindingUrl);
            Assert.Equal(0, handler.CallCount);
        }

        [Fact]
        public async Task SendAsync_WithoutBindingAndUsername_ReturnsNullBindingUrl()
        {
            var handler = FakeHttpMessageHandler.ReturnsOk();
            using var httpClient = new HttpClient(handler);
            using var db = CreateDb();
            var sender = CreateSender(db, httpClient, username: null);

            var result = await sender.SendAsync(Phone, Code, CancellationToken.None);

            Assert.False(result.Delivered);
            Assert.Null(result.BindingUrl);
        }

        [Fact]
        public async Task SendAsync_WhenBotApiFails_Throws()
        {
            var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.BadRequest));
            using var httpClient = new HttpClient(handler);
            using var db = CreateDb(TelegramBinding.Create(Phone, ChatId));
            var sender = CreateSender(db, httpClient);

            await Assert.ThrowsAsync<HttpRequestException>(
                () => sender.SendAsync(Phone, Code, CancellationToken.None));
        }
    }
}
