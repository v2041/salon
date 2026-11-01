namespace Infrastructure.Notifications;

/// <summary>
/// Результат отправки кода. <c>BindingUrl</c> заполняется, когда получатель не привязан.
/// </summary>
public sealed record CodeSendResult(bool Delivered, string? BindingUrl);
