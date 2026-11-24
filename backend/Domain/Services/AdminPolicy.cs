using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Services;

/// <summary>
/// Правило администратора: вход с телефона из настроек даёт роль Admin.
/// </summary>
public static class AdminPolicy
{
    public static void PromoteIfAdminPhone(User user, string? adminPhone)
    {
        if (string.IsNullOrWhiteSpace(adminPhone))
            return;

        if (user.Phone == PhoneNumber.Create(adminPhone))
            user.PromoteToAdmin();
    }
}
