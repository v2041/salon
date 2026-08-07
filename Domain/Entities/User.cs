using Domain.Enums;

namespace Domain.Entities;

public class User
{
    private User()
    {
    }

    private User(
        Guid id,
        long maxId,
        string firstName,
        string? lastName,
        string? username,
        string phone,
        bool isBot,
        DateTime createdAt,
        UserRole role
    )
    {
        Id = id;
        MaxId = maxId;
        FirstName = firstName;
        LastName = lastName;
        Username = username;
        Phone = phone;
        IsBot = isBot;
        CreatedAt = createdAt;
        Role = role;
    }

    public Guid Id { get; }
    public long MaxId { get; private set; }
    public string FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? Username { get; private set; }
    public string Phone { get; private set; }
    public bool IsBot { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public UserRole Role { get; private set; }

    public static User Create(
        long maxId,
        string firstName,
        string? lastName,
        string? username,
        string phone,
        bool isBot)
    {
        return new User(
            Guid.NewGuid(),
            maxId,
            firstName,
            lastName,
            username,
            phone,
            isBot,
            DateTime.UtcNow,
            UserRole.Default
        );
    }
}