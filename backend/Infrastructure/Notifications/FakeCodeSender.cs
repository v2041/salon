namespace Infrastructure.Notifications;

public class FakeCodeSender(ILogger<FakeCodeSender> logger) : ICodeSender
{
    public Task<CodeSendResult> SendAsync(string phone, string code, CancellationToken cancellationToken)
    {
        logger.LogInformation("Код входа для {Phone}: {Code}", phone, code);
        return Task.FromResult(new CodeSendResult(true, null));
    }
}
