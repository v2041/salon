using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class User
{
    /// <summary>Имя клиента по умолчанию.</summary>
    public const string DEFAULT_FIRST_NAME = "Клиент";

    private User()
    {
    }

    private User(
        Guid id,
        string firstName,
        string? lastName,
        PhoneNumber phone,
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
    public string FirstName { get; private set; } = null!;
    public string? LastName { get; private set; }
    public PhoneNumber Phone { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime LastLoginAt { get; private set; }
    public UserRole Role { get; private set; }
    public string? VkId { get; private set; }
    public DateTime? ConsentedAt { get; private set; }
    public string? ConsentPolicyVersion { get; private set; }

    public static User Create(
        string firstName,
        string? lastName,
        PhoneNumber phone,
        DateTime dateTime
    )
    {
        return Create(firstName, lastName, phone, dateTime, Guid.NewGuid());
    }

    public static User Create(
        string firstName,
        string? lastName,
        PhoneNumber phone,
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

    public void LinkVkId(string vkId)
    {
        if (string.IsNullOrWhiteSpace(vkId))
            throw new BusinessException("Пустой идентификатор VK");

        VkId = vkId.Trim();
    }

    public void RecordConsent(string policyVersion, DateTime at)
    {
        if (string.IsNullOrWhiteSpace(policyVersion))
            throw new BusinessException("Пустая версия политики конфиденциальности");

        ConsentPolicyVersion = policyVersion.Trim();
        ConsentedAt = at;
    }

    public void UpdateName(string firstName, string? lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new BusinessException("Имя не может быть пустым");

        FirstName = firstName.Trim();
        LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName.Trim();
    }
}