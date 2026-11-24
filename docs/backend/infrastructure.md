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

Сущности Domain лежат в `Domain/Entities`.

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

## VK ID

`Authentication/VkIdClient.cs` меняет код подтверждения на данные пользователя VK ID.

- Шаг 1 — `POST https://id.vk.ru/oauth2/auth`: `grant_type=authorization_code`, `code`, `code_verifier`, `client_id`, `device_id`, `redirect_uri`. `service_token` уходит, только если задан (`VkId:ServiceToken`).
- Шаг 2 — `POST https://id.vk.ru/oauth2/user_info`: `client_id`, `access_token`. Возвращает `user_id`, имя, фамилию и телефон.
- Таймаут HTTP-клиента — 15 секунд (`HttpClientName = "VkId"`).
- Ошибка API или ответ не в формате JSON — `VkIdException`.
- Токены VK не храним: access token нужен только для чтения профиля.

## Фоновый сервис

`BackgroundServices/ScheduleService.cs` создает расписание по настройкам мастера.
Сервис читает строку `SalonSettings` каждый цикл и повторяет работу примерно каждые 100 секунд.
Общий сценарий генерации и применения лежит в `Services/ScheduleGenerationRunner.cs`.

- Генерация выключена — сервис ничего не меняет.
- Генерация включена — сервис создает отсутствующие дни от завтра до `HorizonDays` и выравнивает Auto-дни без активных записей.
- Для дней из `WeekendDays` сервис ставит `IsWorking=false`. Интервалы сервис берет из настроек.
- Сервис не трогает Manual-дни, дни с активными записями и сегодняшний день.
- Если строки настроек нет, сервис пропускает цикл и пишет предупреждение в лог.

`POST /api/settings/apply` выполняет такой проход немедленно: выравнивает все будущие дни без активных записей и помечает их Auto.

Перед демо-сидом остановите API. Иначе сервис изменит расписание.

Подключение БД — `AddDbContext<SalonDbContext>` в WebApi.
Секреты: `../security.md`.
