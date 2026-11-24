namespace Infrastructure.Authentication;

/// <summary>
/// Данные пользователя из VK ID. Телефон пустой, если доступ к номеру не выдан.
/// </summary>
public sealed record VkUserInfo(string Id, string? FirstName, string? LastName, string? Phone);
