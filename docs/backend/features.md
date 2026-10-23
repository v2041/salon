# Features

HTTP-слой: vertical slices, Minimal API. Одна папка — один сценарий.

## Структура фичи

```text
Features/<Сущность>/<ИмяФичи>/
    Endpoint.cs
    <Имя>Request.cs
    <Имя>Response.cs
    Validator.cs        # если нужна валидация
```

Request и Response — positional record. Namespace — `Features.<Сущность>.<Фича>`.

## Endpoint

- `public static class Endpoint`.
- Один метод `<Фича>Async(...)`. Зависимости — через параметры.
- Валидация → `Results.ValidationProblem`.
- Не найдено → `NotFoundException`. Бизнес-правило → `BusinessException`.
- Дубликат → `Results.Conflict`.
- Успех → `Created` (201), `Ok` (200) или `NoContent` (204).
- `CancellationToken` передавайте в запросы и `SaveChangesAsync`.

```csharp
public static async Task<IResult> CreateOfferingAsync(
    [FromBody] CreateOfferingRequest request,
    IValidator<CreateOfferingRequest> validator,
    SalonDbContext db,
    CancellationToken ct)
{
    var result = await validator.ValidateAsync(request, ct);
    if (!result.IsValid)
        return Results.ValidationProblem(result.ToDictionary());

    var offering = Offering.Create(
        Money.FromDecimal(request.Price),
        request.Title,
        request.Description,
        request.Duration,
        request.Category);

    db.Offerings.Add(offering);
    await db.SaveChangesAsync(ct);

    return Results.Created(
        $"/api/offerings/{offering.Id}",
        new CreateOfferingResponse(
            offering.Id,
            offering.Title,
            offering.Description,
            offering.Price,
            offering.Duration,
            offering.Category));
}
```

Endpoint делает HTTP-работу и сохранение. Бизнес-проверки остаются в Domain.

## Auth

- `userId` — из `HttpContext.User` claims, не из тела (FR-S2).
- `phone` — из claims для маршрутов текущего пользователя.
- Исключения: `send-code`, `login`, `register`, админ-запись — телефон в теле.
- Политики `"Admin"` и `"User"` ставьте в модулях.

## Валидатор

FluentValidation. Сообщения на русском.

```csharp
public class Validator : AbstractValidator<CreateOfferingRequest>
{
    public Validator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Название обязательно.");
    }
}
```

Валидаторы регистрируются сканированием в WebApi. Вручную не регистрируйте.

## Модуль

Один `<Сущность>Module.cs` на сущность.

```csharp
public static class OfferingsModule
{
    public static void MapOfferingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/offerings").WithTags("Offerings");
        group.MapGet("/", GetAllOfferings.Endpoint.GetAllOfferingsAsync);
        group.MapPost("/", CreateOffering.Endpoint.CreateOfferingAsync)
            .RequireAuthorization("Admin");
    }
}
```

Новый модуль подключите в `Program.cs`.

Форматы и ошибки: `../api.md`. Роуты: `README.md` §4.
