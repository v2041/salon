# AGENTS.md — WebApi (backend/WebApi)

Точка входа ASP.NET Core, middleware, конфигурация.

## Program.cs

- Top-level statements. Порядок сервисов важен:
  `AddOpenApi` → `AddDbContext` → JWT Bearer → политики `"Admin"`/`"User"` →
  `AddValidatorsFromAssembly` → `AddHostedService<ScheduleService>` → CORS.
- Middleware строго: `UseCors` → `UseMiddleware<ExceptionHandlingMiddleware>()` →
  `UseAuthentication` → `UseAuthorization`.
- Фичи подключаются модулями: `app.MapOfferingsEndpoints(); app.MapAppointmentsEndpoints(); ...`
  Новый модуль — добавить вызов сюда.

## Auth

- JWT читается из cookie `"some-cookies"` в `OnMessageReceived` (не только Bearer).
- Политики: `"Admin"`, `"User"`. Ставить в модулях фич, не глобально.
- CORS: `localhost:5173`, `localhost:3000`, `AllowCredentials()` (нужен для cookie).

## ExceptionHandlingMiddleware

Не менять формат ответа. Switch:

- `BusinessException` → 400 «Ошибка бизнес-логики»
- `NotFoundException` → 404 «Данные не найдены»
- `JsonException`/`BadHttpRequestException` → 400
- иное → 500 + `logger.LogError`

Контент-тайп `application/problem+json`, message = `exception.Message` (на русском).
Исключение логировать ровно один раз (не дублировать `LogError`).

## Конфигурация

- `appsettings.Development.json` — в `.gitignore`: строка БД, `Jwt:Key`. Никогда не коммитить и не выводить в логи.
- Правки API-контракта — сверять с `README.md` §4 и `TZ.md` §5; новые требования — `TZ.md` §3.
