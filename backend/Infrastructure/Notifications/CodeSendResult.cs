namespace Infrastructure.Notifications;

/// <summary>
/// Результат отправки кода. <c>BindingUrl</c> — ссылка на бота, когда привязки нет и username бота задан.
/// </summary>
public sealed record CodeSendResult(bool Delivered, string? BindingUrl);
