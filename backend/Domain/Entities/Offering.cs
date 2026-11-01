using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities;

public class Offering
{
    public const int MAX_TITLE_LENGTH = 50;
    public const int MIN_DURATION_MINUTES = 10;
    public static readonly string MIN_DURATION_MESSAGE = $"Минимальная длительность услуги - {MIN_DURATION_MINUTES} минут.";
    public static readonly TimeSpan MIN_DURATION = TimeSpan.FromMinutes(MIN_DURATION_MINUTES);

    private Offering()
    {
    }

    private Offering(
        Guid id,
        Money price,
        string title,
        string description,
        TimeSpan duration,
        bool isActive,
        Category category
    )
    {
        Id = id;
        Price = price;
        Title = title;
        Description = description;
        Duration = duration;
        IsActive = isActive;
        Category = category;
    }

    public Guid Id { get; }
    public Money Price { get; private set; } = default!;
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TimeSpan Duration { get; private set; }
    public bool IsActive { get; private set; }
    public Category Category { get; private set; }

    public static Offering Create(Money price, string title, string description, TimeSpan duration, Category category)
    {
        EnsureDuration(duration);
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessException("Название не должно быть пустым.");
        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessException("Описание не должно быть пустым.");
        if (title.Length > MAX_TITLE_LENGTH)
            throw new BusinessException("Название слишком длинное.");
        return new Offering(Guid.NewGuid(), price, title, description, duration, true, category);
    }

    public void ChangeTitle(string newTitle)
    {
        if (newTitle.Length > MAX_TITLE_LENGTH)
            throw new BusinessException("Название слишком длинное.");
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new BusinessException("Название не должно быть пустым.");
        Title = newTitle;
    }

    public void ChangeDuration(TimeSpan newDuration)
    {
        EnsureDuration(newDuration);
        Duration = newDuration;
    }

    public static void EnsureDuration(TimeSpan duration)
    {
        if (duration <= MIN_DURATION)
            throw new BusinessException(MIN_DURATION_MESSAGE);
    }

    public void ChangeDescription(string newDescription)
    {
        if (string.IsNullOrWhiteSpace(newDescription))
            throw new BusinessException("Описание не должно быть пустым.");
        Description = newDescription;
    }

    public void ChangePrice(Money newPrice) => Price = newPrice;
    public void ChangeCategory(Category newCategory) => Category = newCategory;
    public void Remove() => IsActive = false;
    public void Restore() => IsActive = true;
}
