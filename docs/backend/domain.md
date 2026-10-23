# Domain

Бизнес-правила живут только в Domain. Domain не ссылается на Infrastructure.

## Структура

- `Entities/` — сущности.
- `ValueObjects/` — Value Objects.
- `Enums/` — enum'ы.
- `Services/` — статические доменные сервисы.
- `Exceptions/` — `BusinessException`, `NotFoundException`.

## Сущности

Паттерн: приватный конструктор для EF, приватный полный конструктор и статическая фабрика `Create`.

```csharp
public class Offering
{
    public const int MAX_TITLE_LENGTH = 50;

    public Guid Id { get; }
    public string Title { get; private set; } = null!;

    private Offering() { }

    private Offering(
        Guid id,
        Money price,
        string title,
        string description,
        TimeSpan duration,
        bool isActive,
        Category category)
    {
        Id = id;
        Price = price;
        Title = title;
        Description = description;
        Duration = duration;
        IsActive = isActive;
        Category = category;
    }

    public static Offering Create(
        Money price,
        string title,
        string description,
        TimeSpan duration,
        Category category)
    {
        if (title.Length > MAX_TITLE_LENGTH)
            throw new BusinessException($"Максимальная длина названия - {MAX_TITLE_LENGTH} символов.");

        return new Offering(Guid.NewGuid(), price, title, description, duration, true, category);
    }
}
```

Правила:

- Сущность — `class`, не `record`.
- `Id` — `{ get; }`. Остальные свойства — `{ get; private set; }`.
- Проверки — в фабрике и доменных методах.
- Исключение — `BusinessException` с русским сообщением.
- Лимиты — `public const` в сущности.
- Коллекции — `private readonly List<T>` и `IReadOnlyCollection<T>`.
- Мутации — только через доменные методы. Публичных сеттеров нет.
- Производные значения — вычисляемые свойства.

## Value Objects

Value Object — `record` с `[JsonConstructor]` приватным конструктором и статической фабрикой.

- `Money.FromDecimal(value, currency = "RUB")`.
- `TimeInterval.Create(start, end)`. Методы `IsOverlapping`, `IsInside`, свойство `Duration`.
- Проверки времени и дат — в Value Objects.

## Время

- `Schedule.AddAppointment` по умолчанию запрещает прошлое. Флаг `checkPast: false` нужен только для импорта и демо-данных.
- Не добавляйте длительность к `TimeOnly` без проверки. `TimeOnly.Add` переходит через полночь.
  Сравните `current > End` и `duration > End - current`.

## Доменные сервисы

`static`-классы в `Services/`. Без состояния, EF и HTTP.

## Исключения

Однострочные классы:

```csharp
public class BusinessException(string message) : Exception(message);
```

Сообщения — на русском.

## Запреты

- Не кладите EF, HTTP и DTO в Domain.
- Не меняйте числа enum'ов `AppointmentStatus`, `Category`, `UserRole`.
- Не мутируйте сущность вне доменных методов.
