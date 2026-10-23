# Infrastructure

EF Core, PostgreSQL, JWT и фоновые сервисы.

## EF-конфигурации

Одна конфигурация на сущность: `Configurations/<Сущность>Configuration.cs`.

Регистрация — вручную в `SalonDbContext.OnModelCreating`:

```csharp
modelBuilder.ApplyConfiguration(new OfferingConfiguration());
```

Не используйте `ApplyConfigurationsFromAssembly`.

Value Objects маппятся через `OwnsOne`:

- `Money.Value` → колонка `Price` у `Offering` и `AppointmentOffering`.
- `TimeInterval` → `StartTime`/`EndTime` у записи.
- `Schedule` → `WorkStartTime`/`WorkEndTime` и `BreakStartTime`/`BreakEndTime`.

Не переиспользуйте один экземпляр owned `Money` в нескольких сущностях. EF отслеживает экземпляр.
Создавайте новый `Money` для каждой строки.

Явные FK и `DeleteBehavior.Restrict` сохраняйте.

Сущности Domain лежат в `Domain/Entities`. Инфраструктурные сущности (например `VerificationCode`) — в `Infrastructure/Entities`.

## Миграции

Рабочая папка — `backend/`.

```powershell
dotnet ef migrations add <Name> --project Infrastructure --startup-project WebApi
dotnet ef database update --project Infrastructure --startup-project WebApi
```

Готовые миграции не редактируйте. Добавляйте новую миграцию.
Миграция нужна после изменения конфигурации или сущности Domain.

## JWT

`Authentication/JwtTokenGenerator.cs` выпускает JWT. Cookie — `some-cookies`, HttpOnly.

## Фоновый сервис

`BackgroundServices/ScheduleService.cs` создает расписание на 14 дней вперед.
Часы работы: 10:00–18:00. Перерыв: 15:00–15:20. Воскресенье — выходной.
Сервис повторяет работу примерно каждые 100 секунд.

Перед демо-сидом остановите API. Иначе сервис изменит расписание.

Подключение БД — `AddDbContext<SalonDbContext>` в WebApi.
Секреты: `../security.md`.
