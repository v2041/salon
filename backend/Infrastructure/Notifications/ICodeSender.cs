namespace Infrastructure.Notifications;

public interface ICodeSender
{
    Task<CodeSendResult> SendAsync(string phone, string code, CancellationToken cancellationToken);
}
