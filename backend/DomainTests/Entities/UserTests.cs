using Domain.Entities;
using Domain.Enums;

namespace DomainTests.Entities;

public class UserTests
{
    public class Create
    {
        [Fact]
        public void Create_Always_SetsDefaultRole()
        {
            var user = User.Create("Имя", "Фамилия", "+79990000000", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            Assert.Equal(UserRole.Default, user.Role);
        }
    }

    public class PromoteToAdmin
    {
        private readonly User _user = User.Create(
            "Имя",
            "Фамилия",
            "+79990000000",
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
}
