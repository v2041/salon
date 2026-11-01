using System.Text.Json;
using Infrastructure.Data;
using Infrastructure.Entities;
using Infrastructure.Notifications;
using Microsoft.EntityFrameworkCore;

namespace DomainTests.Notifications;

public class TelegramUpdateHandlerTests
{
    private const string Phone = "+79991234567";
    private const long ChatId = 555L;
    private const long OtherId = 777L;

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

    private static TelegramUpdateHandler CreateHandler(
        SalonDbContext db,
        RecordingSender sender)
        => new(db, sender);

    private sealed class RecordingSender : ITelegramMessageSender
    {
        public List<SentMessage> Messages { get; } = [];

        public Task SendMessageAsync(
            long chatId,
            string text,
            object? replyMarkup,
            CancellationToken cancellationToken)
        {
            Messages.Add(new SentMessage(chatId, text, replyMarkup));
            return Task.CompletedTask;
        }
    }

    private sealed record SentMessage(long ChatId, string Text, object? ReplyMarkup);

    public class HandleAsync
    {
        [Fact]
        public async Task HandleAsync_Start_RepliesWithRequestContactKeyboard()
        {
            var sender = new RecordingSender();
            using var db = CreateDb();
            var handler = CreateHandler(db, sender);

            await handler.HandleAsync(
                new TelegramUpdate(1, new TelegramMessage(ChatId, ChatId, "/start", null)),
                CancellationToken.None);

            var message = Assert.Single(sender.Messages);
            Assert.Equal(ChatId, message.ChatId);
            Assert.Equal(
                "Здравствуйте! Нажмите «Поделиться номером», чтобы привязать номер телефона.",
                message.Text);
            using var markup = JsonDocument.Parse(JsonSerializer.Serialize(message.ReplyMarkup));
            var button = markup.RootElement.GetProperty("keyboard")[0][0];
            Assert.Equal("Поделиться номером", button.GetProperty("text").GetString());
            Assert.True(button.GetProperty("request_contact").GetBoolean());
        }

        [Fact]
        public async Task HandleAsync_OtherMessage_RepliesWithHint()
        {
            var sender = new RecordingSender();
            using var db = CreateDb();
            var handler = CreateHandler(db, sender);

            await handler.HandleAsync(
                new TelegramUpdate(2, new TelegramMessage(ChatId, ChatId, "привет", null)),
                CancellationToken.None);

            var message = Assert.Single(sender.Messages);
            Assert.Equal(ChatId, message.ChatId);
            Assert.Equal(
                "Нажмите «Поделиться номером», чтобы привязать номер телефона.",
                message.Text);
        }

        [Fact]
        public async Task HandleAsync_ContactFromOtherUser_DoesNotBind()
        {
            var sender = new RecordingSender();
            using var db = CreateDb();
            var handler = CreateHandler(db, sender);
            var contact = new TelegramContact(Phone, OtherId);

            await handler.HandleAsync(
                new TelegramUpdate(3, new TelegramMessage(ChatId, ChatId, null, contact)),
                CancellationToken.None);

            Assert.Empty(db.TelegramBindings);
            Assert.Empty(sender.Messages);
        }

        [Fact]
        public async Task HandleAsync_OwnContact_CreatesBinding()
        {
            var sender = new RecordingSender();
            using var db = CreateDb();
            var handler = CreateHandler(db, sender);
            var contact = new TelegramContact(Phone, ChatId);

            await handler.HandleAsync(
                new TelegramUpdate(4, new TelegramMessage(ChatId, ChatId, null, contact)),
                CancellationToken.None);

            var binding = Assert.Single(db.TelegramBindings);
            Assert.Equal(Phone, binding.Phone);
            Assert.Equal(ChatId, binding.ChatId);
            var message = Assert.Single(sender.Messages);
            Assert.Equal(ChatId, message.ChatId);
            Assert.Equal(
                "Номер привязан. Вернитесь на сайт и нажмите «Получить код».",
                message.Text);
            Assert.Null(message.ReplyMarkup);
        }

        [Fact]
        public async Task HandleAsync_RepeatedContact_UpdatesChatId()
        {
            const long newChatId = 888L;
            var sender = new RecordingSender();
            using var db = CreateDb(TelegramBinding.Create(Phone, ChatId));
            var handler = CreateHandler(db, sender);
            var contact = new TelegramContact(Phone, newChatId);

            await handler.HandleAsync(
                new TelegramUpdate(5, new TelegramMessage(newChatId, newChatId, null, contact)),
                CancellationToken.None);

            var binding = Assert.Single(db.TelegramBindings);
            Assert.Equal(newChatId, binding.ChatId);
        }

        [Fact]
        public async Task HandleAsync_ContactWithLocalPhoneFormat_NormalizesPhone()
        {
            var sender = new RecordingSender();
            using var db = CreateDb();
            var handler = CreateHandler(db, sender);
            var contact = new TelegramContact("8 (999) 123-45-67", ChatId);

            await handler.HandleAsync(
                new TelegramUpdate(6, new TelegramMessage(ChatId, ChatId, null, contact)),
                CancellationToken.None);

            var binding = Assert.Single(db.TelegramBindings);
            Assert.Equal(Phone, binding.Phone);
        }
    }
}
