using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace DomainTests.Entities;

public class UserTests
{
    public class Create
    {
        [Fact]
        public void Create_Always_SetsDefaultRole()
        {
            var user = User.Create("Имя", "Фамилия", PhoneNumber.Create("+79990000000"), new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            Assert.Equal(UserRole.Default, user.Role);
        }
    }

    public class PromoteToAdmin
    {
        private readonly User _user = User.Create(
            "Имя",
            "Фамилия",
            PhoneNumber.Create("+79990000000"),
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        [Fact]
        public void PromoteToAdmin_WithDefaultUser_SetsAdminRole()
        {
            _user.PromoteToAdmin();

            Assert.Equal(UserRole.Admin, _user.Role);
        }

        [Fact]
        public void PromoteToAdmin_WhenAlreadyAdmin_KeepsAdminRole()
        {
            _user.PromoteToAdmin();
            _user.PromoteToAdmin();

            Assert.Equal(UserRole.Admin, _user.Role);
        }
    }

    public class LinkVkId
    {
        private readonly User _user = User.Create(
            "Имя",
            "Фамилия",
            PhoneNumber.Create("+79990000000"),
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        [Fact]
        public void LinkVkId_WithId_SetsVkId()
        {
            _user.LinkVkId("1234567890");

            Assert.Equal("1234567890", _user.VkId);
        }

        [Fact]
        public void LinkVkId_WhenAlreadyLinked_ReplacesVkId()
        {
            _user.LinkVkId("1234567890");

            _user.LinkVkId("9876543210");

            Assert.Equal("9876543210", _user.VkId);
        }

        [Fact]
        public void LinkVkId_WithSpaces_TrimsId()
        {
            _user.LinkVkId("  1234567890  ");

            Assert.Equal("1234567890", _user.VkId);
        }

        [Fact]
        public void LinkVkId_WithEmptyId_ThrowsBusinessException()
        {
            Assert.Throws<BusinessException>(() => _user.LinkVkId("   "));
        }
    }

    public class RecordConsent
    {
        private static readonly DateTime ConsentTime = new(2026, 11, 24, 12, 0, 0, DateTimeKind.Utc);

        private readonly User _user = User.Create(
            "Имя",
            "Фамилия",
            PhoneNumber.Create("+79990000000"),
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        [Fact]
        public void RecordConsent_WithVersion_SetsVersionAndTime()
        {
            _user.RecordConsent("2026-11-24", ConsentTime);

            Assert.Equal("2026-11-24", _user.ConsentPolicyVersion);
            Assert.Equal(ConsentTime, _user.ConsentedAt);
        }

        [Fact]
        public void RecordConsent_WhenCalledAgain_UpdatesVersionAndTime()
        {
            var nextTime = ConsentTime.AddDays(30);
            _user.RecordConsent("2026-11-24", ConsentTime);

            _user.RecordConsent("2027-01-01", nextTime);

            Assert.Equal("2027-01-01", _user.ConsentPolicyVersion);
            Assert.Equal(nextTime, _user.ConsentedAt);
        }

        [Fact]
        public void RecordConsent_WithEmptyVersion_ThrowsBusinessException()
        {
            Assert.Throws<BusinessException>(() => _user.RecordConsent(" ", ConsentTime));
        }
    }

    public class UpdateName
    {
        private readonly User _user = User.Create(
            "Имя",
            "Фамилия",
            PhoneNumber.Create("+79990000000"),
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        [Fact]
        public void UpdateName_WithBothNames_SetsNames()
        {
            _user.UpdateName("Анна", "Иванова");

            Assert.Equal("Анна", _user.FirstName);
            Assert.Equal("Иванова", _user.LastName);
        }

        [Fact]
        public void UpdateName_WithSpaces_TrimsNames()
        {
            _user.UpdateName("  Анна  ", "  Иванова  ");

            Assert.Equal("Анна", _user.FirstName);
            Assert.Equal("Иванова", _user.LastName);
        }

        [Fact]
        public void UpdateName_WithEmptyLastName_StoresNull()
        {
            _user.UpdateName("Анна", "  ");

            Assert.Null(_user.LastName);
        }

        [Fact]
        public void UpdateName_WithEmptyFirstName_ThrowsBusinessException()
        {
            Assert.Throws<BusinessException>(() => _user.UpdateName(" ", "Иванова"));
        }
    }
}
