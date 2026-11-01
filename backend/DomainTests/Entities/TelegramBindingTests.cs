using Infrastructure.Entities;

namespace DomainTests.Entities;

public class TelegramBindingTests
{
    private const string Phone = "+79991234567";
    private const long ChatId = 123456789L;

    public class Create
    {
        [Fact]
        public void Create_WithPhoneAndChatId_StoresBoth()
        {
            var binding = TelegramBinding.Create(Phone, ChatId);

            Assert.Equal(Phone, binding.Phone);
            Assert.Equal(ChatId, binding.ChatId);
        }
    }

    public class Rebind
    {
        [Fact]
        public void Rebind_WithNewChatId_UpdatesChatIdAndUpdatedAt()
        {
            var binding = TelegramBinding.Create(Phone, ChatId);
            var updatedAt = binding.UpdatedAt;
            const long newChatId = 987654321L;

            binding.Rebind(newChatId, updatedAt.AddMinutes(5));

            Assert.Equal(newChatId, binding.ChatId);
            Assert.Equal(updatedAt.AddMinutes(5), binding.UpdatedAt);
        }
    }
}
