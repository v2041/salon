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

Сущности Domain лежат в `Domain/Entities`. Инфраструктурные сущности (например `TelegramBinding`) — в `Infrastructure/Entities`.

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

## Уведомления

`Notifications/` доставляет код входа в Telegram.

- Шов — `ICodeSender.SendAsync`. `FakeCodeSender` работает в Development без токена бота и пишет код в лог. `TelegramBotCodeSender` ищет чат по `TelegramBinding` и шлёт «Код для входа: 1234. Никому не сообщайте.». Без привязки возвращает `BindingUrl` — ссылку `https://t.me/<username>`.
- `TelegramBotClient` — обёртка над Bot API: `sendMessage` и `getUpdates`. `sendMessage` завершается через 5 секунд. Ответ с `ok != true` или не в формате JSON — исключение.
- `TelegramUpdateHandler` обрабатывает `/start` (кнопка «Поделиться номером») и контакт. Бот принимает контакт, только если его отправил владелец. Повторный контакт обновляет `chat_id`.
- `TelegramUpdatesService` — hosted service: long polling (`getUpdates`, timeout 50 секунд). Ошибка не останавливает сервис: пауза 5 секунд.

Сбой провайдера — исключение. Код сохраняется только после успешной отправки.

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
