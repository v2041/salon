using Domain.ValueObjects;

namespace Infrastructure.Notifications;

public interface ICodeSender
{
    Task<CodeSendResult> SendAsync(PhoneNumber phone, string code, CancellationToken cancellationToken);
}
