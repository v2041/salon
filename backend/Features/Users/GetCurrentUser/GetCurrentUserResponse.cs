using Domain.Enums;

namespace Features.Users.GetCurrentUser;

public record GetCurrentUserResponse(
    Guid Id,
    string FirstName,
    string? LastName,
    string Phone,
    UserRole Role
);
