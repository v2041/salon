namespace Features.Users.Register;

public record RegisterRequest(
    string Code,
    string Phone,
    string FirstName,
    string LastName
);