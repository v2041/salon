using Domain.Entities;
using Domain.Enums;
using Domain.Services;
using Domain.ValueObjects;

namespace DomainTests.Services;

public class AdminPolicyTests
{
    private static readonly DateTime Now = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static User CreateUser(string phone) =>
        User.Create("Имя", null, PhoneNumber.Create(phone), Now);

    public class PromoteIfAdminPhone
    {
        [Fact]
        public void PromoteIfAdminPhone_WhenPhoneMatches_PromotesToAdmin()
        {
            var user = CreateUser("+79991234567");

            AdminPolicy.PromoteIfAdminPhone(user, "+79991234567");

            Assert.Equal(UserRole.Admin, user.Role);
        }

        [Fact]
        public void PromoteIfAdminPhone_WhenPhoneDiffers_KeepsDefaultRole()
        {
            var user = CreateUser("+79990000000");

            AdminPolicy.PromoteIfAdminPhone(user, "+79991234567");

            Assert.Equal(UserRole.Default, user.Role);
        }

        [Fact]
        public void PromoteIfAdminPhone_WhenAdminPhoneIsEmpty_KeepsDefaultRole()
        {
            var user = CreateUser("+79990000000");

            AdminPolicy.PromoteIfAdminPhone(user, "  ");

            Assert.Equal(UserRole.Default, user.Role);
        }

        [Fact]
        public void PromoteIfAdminPhone_WhenPhoneHasDifferentFormat_PromotesToAdmin()
        {
            var user = CreateUser("+79991234567");

            AdminPolicy.PromoteIfAdminPhone(user, "8 (999) 123-45-67");

            Assert.Equal(UserRole.Admin, user.Role);
        }
    }
}
