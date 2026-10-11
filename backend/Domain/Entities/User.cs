using Domain.Enums;

namespace Domain.Entities;

public class User
{
    private User()
    {
    }

    private User(
        Guid id,
        string firstName,
        string? lastName,
        string phone,
        DateTime dateTime,
        UserRole role
    )
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        CreatedAt = dateTime;
        LastLoginAt = dateTime;
        Role = role;
    }

    public Guid Id { get; }
    public string FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string Phone { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime LastLoginAt { get; private set; }
    public UserRole Role { get; private set; }

    public static User Create(
        string firstName,
        string? lastName,
        string phone,
        DateTime dateTime
    )
    {
        return Create(firstName, lastName, phone, dateTime, Guid.NewGuid());
    }

    public static User Create(
        string firstName,
        string? lastName,
        string phone,
        DateTime dateTime,
        Guid id
    )
    {
        return new User(
            id,
            firstName,
            lastName,
            phone,
            dateTime,
            UserRole.Default
        );
    }

    public void PromoteToAdmin()
    {
        if (Role == UserRole.Admin)
            return;

        Role = UserRole.Admin;
    }

    public void MarkLoggedIn()
    {
        LastLoginAt = DateTime.UtcNow;
    }
}