# AGENTS.md — Features (backend/Features)

HTTP-слой: vertical slices, Minimal API. Одна фича = один сценарий.

## Структура фичи

```
Features/<Сущность>/<ИмяФичи>/
    Endpoint.cs
    <Имя>Request.cs
    <Имя>Response.cs
    Validator.cs        # необязательно, если валидация не нужна
```

Примеры: `Offerings/CreateOffering/`, `Appointments/CreateAppointment/`.

## Request / Response

Positional record, namespace = `Features.<Сущность>.<Фича>`:

```csharp
public record CreateOfferingRequest(decimal Price, string Title, string Description, TimeSpan Duration, Category Category);
public record CreateOfferingResponse(Guid Id, /* ... */);
```

## Endpoint

```csharp
public static class Endpoint
{
    public static async Task<IResult> CreateOfferingAsync(
        [FromBody] CreateOfferingRequest request,
        IValidator<CreateOfferingRequest> validator,
        SalonDbContext db,
        CancellationToken ct)
    {
        var errors = validator.Validate(request);
        if (!errors.IsValid)
            return Results.ValidationProblem(errors.ToDictionary());

        var offering = Offering.Create( /* поля из request */ );
        db.Offerings.Add(offering);
        await db.SaveChangesAsync(ct);

        return Results.Created($"api/offerings/{offering.Id}", new CreateOfferingResponse(/* */));
    }
}
```

Правила:

- `public static class Endpoint`, один метод `<Фича>Async(...)`, DI через параметры.
- `userId`/`phone` — из `HttpContext.User` claims, НЕ из тела запроса (FR-S2).
- Ошибки: валидация → `Results.ValidationProblem`; не найдено → `throw new NotFoundException`;
  бизнес-правило → `throw new BusinessException` (обработает middleware);
  дубликат → `Results.Conflict()`. Успех: `Created` (201), `Ok` (200), `NoContent` (204).
- Бизнес-логика — через доменные методы (`Offering.Create`, `schedule.AddAppointment`).
  В Features — только HTTP + EF save. Никаких проверок правил в Endpoint'ах.
- `CancellationToken` — везде, где есть `SaveChanges`/запросы.

## Валидатор (FluentValidation)

```csharp
public class Validator : AbstractValidator<CreateOfferingRequest>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.Title).NotEmpty().MaximumLength(50).WithMessage("Название обязательно.");
    }
}
```

Сообщения на русском. Регистрация — общее сканирование в WebApi, вручную не регистрировать.

## Модуль (регистрация роутов)

`Features/<Сущность>/<Сущность>Module.cs`:

```csharp
public static class OfferingsModule
{
    public static void MapOfferingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/offerings").WithTags("Offerings");
        group.MapGet("/", GetAllOfferings.Endpoint.GetAllOfferingsAsync);
        group.MapPost("/", CreateOffering.Endpoint.CreateOfferingAsync).RequireAuthorization("Admin");
    }
}
```

- Модуль один на сущность. Политики: `"Admin"`, `"User"` (объявлены в WebApi/Program.cs).
- Готовые, но незарегистрированные фичи: `ChangeAppointmentTime`, `ChangeAppointmentOfferingPrice`,
  `ChangeOffering` — при подключении добавлять в модуль, не переписывать.
