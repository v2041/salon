# AGENTS.md — DomainTests (backend/DomainTests)

xUnit-тесты доменного слоя. Тестируем Domain, не HTTP.

## Стек

- xUnit (`[Fact]`, `[Theory]`), `Assert.*` из коробки.
- FluentAssertions НЕ использовать — только `Assert`.
- Global using `Xunit` уже настроен в csproj.

## Структура

```
DomainTests/Entities/       OfferingTests, ScheduleTests
DomainTests/Value Objects/  MoneyTests, TimeIntervalTests
```

Новый тест — в папку по слою, файл `<Сущность>Tests.cs`.

## Группировка и naming

Вложенные классы по методу, имя метода `Method_Condition_Expectation`:

```csharp
public class OfferingTests
{
    public class Create
    {
        [Fact]
        public void Create_WithValid_ReturnsOffering() { ... }

        [Theory]
        [InlineData("", 30)]
        public void Create_WithEmptyTitle_ThrowsBusinessException(string title, int minutes)
            => Assert.Throws<BusinessException>(() =>
                Offering.Create(Money.FromDecimal(1000), title, minutes));
    }

    public class ChangeTitle { ... }
}
```

Правила:

- Arrange: общие данные — в readonly-поля класса, разовые — инлайн. Комментарии `// Arrange` не нужны.
- Бизнес-правило → `Assert.Throws<BusinessException>(...)`, `NotFoundException` — так же.
- Каждое правило из Domain покрыть:happy path + нарушение. Особенно: пересечение слотов,
  запись в перерыв/прошлое, отмена < 2 часов, длина названия, минимальная длительность.
- Тесты — изолированные, без БД и времени «сейчас» (передавать даты явно).
- Новый доменный метод → сразу тесты здесь, до правок Features.
