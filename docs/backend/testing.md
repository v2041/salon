# Тесты backend

`DomainTests` — xUnit-тесты доменного слоя и сущности `VerificationCode`.
`WebApiTests` — интеграционные HTTP-тесты на PostgreSQL через Testcontainers.

## Стек

- xUnit: `[Fact]`, `[Theory]`, `Assert.*`.
- FluentAssertions не использовать.
- `Xunit` подключен через global using.

## Структура

- `Entities/` — тесты сущностей.
- `Value Objects/` — тесты Value Objects.
- Файл — `<Сущность>Tests.cs`.

`VerificationCode` лежит в Infrastructure. Ссылка на Infrastructure нужна только для его тестов.

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
- Тесты изолированные. Без БД и HTTP.

## Время

Для проверки прошлого и будущего можно брать даты от `DateTime.Now` или `DateTime.UtcNow`.
Не зависьте от времени суток. Не проверяйте точное текущее время.
Явные даты передавайте в доменный метод, когда это возможно.

## HTTP-тесты WebApiTests

`WebApiTests` проверяет HTTP-контракт на `WebApplicationFactory` и PostgreSQL через Testcontainers.
Docker Desktop обязателен. Каждый тест получает чистое состояние БД.
Тесты лежат в `WebApiTests/<Раздел>/`. Базовый класс — `Infrastructure/HttpTestBase.cs`.

## Запуск

```powershell
cd backend
dotnet test
```

Число тестов в документации не храните.
