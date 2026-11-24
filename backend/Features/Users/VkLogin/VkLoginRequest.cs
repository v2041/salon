namespace Features.Users.VkLogin;

public record VkLoginRequest(
    string Code,
    string DeviceId,
    string CodeVerifier,
    string RedirectUrl,
    bool Consent
);
