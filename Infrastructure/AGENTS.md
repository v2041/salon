# AGENTS.md — Infrastructure (backend/Infrastructure)

EF Core, PostgreSQL, JWT, фоновые сервисы.

## Конфигурации сущностей

`Configurations/<Сущность>Configuration.cs`, по одной на сущность:

```csharp
public class OfferingConfiguration : IEntityTypeConfiguration<Offering>
{
    public void Configure(EntityTypeBuilder<Offering> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();
        builder.OwnsOne(o => o.Price, b => b.Property(m => m.Value).HasColumnName("Price"));
    }
}
```

- Регистрация — вручную в `SalonDbContext.OnModelCreating`: `modelBuilder.ApplyConfiguration(new OfferingConfiguration());`
  (не `ApplyConfigurationsFromAssembly`).
- Value Objects — через `OwnsOne`: `Money` → колонка `Value`, `TimeInterval` → `StartTime`/`EndTime`.
- Явные FK и `DeleteBehavior.Restrict` там, где уже есть.
- Сущности — только из `Domain/Entities`. Инфраструктурные (например `VerificationCode`) — своя папка `Infrastructure/Entities/`.

## Миграции

- Создавать: `dotnet ef migrations add <Name> --project Infrastructure --startup-project WebApi` (из `backend/`).
- Готовые миграции в `Migrations/` — НЕ редактировать, только новыми.
- Новая миграция — после правки конфигураций/сущностей Domain.

## Прочее

- `Authentication/JwtTokenGenerator.cs` — выпуск JWT, cookie `some-cookies`, HttpOnly.
- `BackgroundServices/ScheduleService.cs` — hosted service, генерация расписания на 14 дней.
- Подключение БД — `AddDbContext<SalonDbContext>(o => o.UseNpgsql(...))` в WebApi.
