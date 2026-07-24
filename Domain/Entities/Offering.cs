using Domain.Exceptions;

namespace Domain.Entities;

public class Offering
{
    private const int MAX_TITLE_LENGTH = 50;

    private Offering()
    {
    }

    private Offering(
        Guid id,
        decimal price,
        string title,
        string description,
        TimeSpan duration,
        DateTime createDate,
        bool isActive
    )
    {
        Id = id;
        Price = price;
        Title = title;
        Description = description;
        Duration = duration;
        CreateDate = createDate;
        IsActive = isActive;
    }

    public Guid Id { get; }
    public decimal Price { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public TimeSpan Duration { get; private set; }
    public DateTime CreateDate { get; private set; }
    public bool IsActive { get; private set; }
    public List<Appointment> Appointments { get; } = new();

    public static Offering Create(decimal price, string title, string description, TimeSpan duration)
    {
        if (title.Length > MAX_TITLE_LENGTH)
            throw new BusinessException("Название слишком длинное.");

        return new Offering(Guid.NewGuid(), price, title, description, duration, DateTime.UtcNow, true);
    }
    
    public void ChangeTitle(string newTitle)
    {
        if (newTitle.Length > MAX_TITLE_LENGTH)
            throw new BusinessException("Название слишком длинное.");

        Title = newTitle;
    }

    public void ChangeDescription(string newDescription) => Description = newDescription;
    public void ChangePrice(decimal newPrice) => Price = newPrice;
    public void Remove() => IsActive = false;
    public void Restore() => IsActive = true;
}