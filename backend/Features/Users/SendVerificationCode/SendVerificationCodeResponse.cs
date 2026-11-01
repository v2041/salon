namespace Features.Users.SendVerificationCode;

public record SendVerificationCodeResponse(
    DateTime ExpiresAt,
    bool IsRegistered,
    string Status,
    string? BindingUrl,
    string? Code
)
{
    public const string StatusSent = "sent";
    public const string StatusBindingRequired = "binding_required";
}
