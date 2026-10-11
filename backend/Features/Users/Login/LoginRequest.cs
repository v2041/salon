namespace Features.Users.Login;

public record LoginRequest(
    string Code,
    string Phone
);