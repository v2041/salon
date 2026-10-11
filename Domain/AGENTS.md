# AGENTS.md — Domain (backend/Domain)

Правила доменного слоя. Бизнес-правила живут ТОЛЬКО здесь.

## Структура

- `Entities/` — сущности, `ValueObjects/` — VO, `Enums/` — enum'ы,
  `Services/` — доменные сервисы (static), `Exceptions/` — `BusinessException`, `NotFoundException`.

## Сущность (паттерн: приватный конструктор + фабрика)

```csharp
public class Offering
{
    public const int MAX_TITLE_LENGTH = 50;
    public const int MIN_DURATION_MINUTES = 10;

    public Guid Id { get; }
    public string Title { get; private set; }
    private readonly List<AppointmentOffering> _offerings = new();
    public IReadOnlyCollection<AppointmentOffering> Offerings => _offerings;

    private Offering() { }                       // для EF
    private Offering(Guid id, ...) { /* assign */ }

    public static Offering Create(Money price, string title, ...)
    {
        if (title.Length > MAX_TITLE_LENGTH)
            throw new BusinessException($"Максимальная длина названия - {MAX_TITLE_LENGTH} символов.");
        return new Offering(Guid.NewGuid(), ...);
    }
}
```

Правила:

- `class`, не `record`. Свойства: `Id` — `{ get; get; }` без сеттера, остальные `{ get; private set; }`.
- Приватный конструктор для EF + приватный полный + статическая фабрика `Create`.
- Все проверки — во фабрике и доменных методах, исключение `BusinessException` с русским сообщением.
- Лимиты — `public const` в сущности, не хардкод в методах.
- Коллекции: `private readonly List<T> _x` + `IReadOnlyCollection<T> X => _x`.
- Мутации — только через доменные методы (`Cancel/Confirm/ChangeTitle/...`). Никаких публичных сеттеров.
- Производные значения — вычисляемые свойства: `public Money Price => Money.FromDecimal(...)`.

## Value Objects

`record` с `[JsonConstructor]` приватным конструктором и статической фабрикой:

- `Money.FromDecimal(value, currency = "RUB")`, свойства `init`.
- `TimeInterval.Create(start, end)`, методы `IsOverlapping`, `IsInside`, свойство `Duration`.
- Проверка времени/дат — через VO, не в сущности.

## Доменные сервисы

`static`-классы в `Services/` (например `TimelineBuilder`, `AppointmentTimeChanger`).
Без состояния, без EF, без HTTP.

## Исключения

Однострочники: `public class BusinessException(string message) : Exception(message);`
Новые правила — всегда с сообщением на русском.

## Чего не делать

- НЕ класть EF/HTTP/DTO в Domain (сборка Domain не ссылается на Infrastructure).
- НЕ менять enum'ы `AppointmentStatus`, `Category`, `UserRole` — их числа закреплены в API.
- НЕ мутировать состояние сущности вне доменных методов (ни в Features, ни в тестах).
