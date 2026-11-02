# DomainTests

xUnit-тесты доменного слоя, `VerificationCode` и классов Infrastructure (`Notifications/`). Реальные БД и сеть не используются.

## Стек

- xUnit: `[Fact]`, `[Theory]`, `Assert.*`.
- FluentAssertions не использовать.
- `Xunit` подключен через global using.

## Структура

- `Entities/` — тесты сущностей.
- `Value Objects/` — тесты Value Objects.
- `Notifications/` — тесты уведомлений: отправители кода, клиент Bot API, обработчик обновлений.
- Файл — `<Класс>Tests.cs`.

`VerificationCode` и классы уведомлений лежат в Infrastructure. Ссылка на Infrastructure нужна для их тестов.

## Naming

Вложенный класс на метод. Имя теста — `Method_Condition_Expectation`.

```csharp
public class OfferingTests
{
    public class Create
    {
        [Fact]
        public void Create_WithValid_ReturnsOffering() { }

        [Theory]
        [InlineData("")]
        public void Create_WithEmptyTitle_ThrowsBusinessException(string title)
            => Assert.Throws<BusinessException>(() =>
                Offering.Create(
                    Money.FromDecimal(100),
                    title,
                    "description",
                    TimeSpan.FromMinutes(30),
                    Category.Hair));
    }
}
```

Правила:

- Общие данные — в readonly-полях класса. Разовые — инлайн.
- Комментарии `// Arrange` не нужны.
- Нарушение правила → `Assert.Throws<BusinessException>` или `Assert.Throws<NotFoundException>`.
- Каждое правило покрывайте happy path и нарушением.
- Проверяйте пересечение слотов, перерыв, прошлое, отмену менее чем за 2 часа, длину названия и минимальную длительность.
- Новый доменный метод → тесты сразу.
- Тесты изолированные. Вместо БД — EF InMemory, вместо сети — подмена `HttpMessageHandler`.

## Время

Для проверки прошлого и будущего можно брать даты от `DateTime.Now` или `DateTime.UtcNow`.
Не зависьте от времени суток. Не проверяйте точное текущее время.
Явные даты передавайте в доменный метод, когда это возможно.

## Запуск

```powershell
cd backend
dotnet test
```

Число тестов в документации не храните.
