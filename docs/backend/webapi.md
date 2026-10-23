# WebApi

Точка входа ASP.NET Core: `Program.cs`, middleware, auth и конфигурация.

## Program.cs

- Top-level statements.
- Зарегистрируйте CORS, `SalonDbContext`, JWT Bearer, политики, валидаторы и `ScheduleService`.
- Валидаторы — `AddValidatorsFromAssembly(typeof(AppointmentsModule).Assembly)`.
- Явно подключите модули: `app.MapOfferingsEndpoints()` и другие.
- Новый модуль требует вызова в `Program.cs`.

Порядок middleware:

```text
UseCors → ExceptionHandlingMiddleware → UseAuthentication → UseAuthorization
```

## Auth

- JWT читается из cookie `some-cookies` в `OnMessageReceived`.
- Политики: `"Admin"` и `"User"`.
- Политики ставьте в модулях фич, не глобально.
- CORS: origins из `Cors:AllowedOrigins`. Fallback — `localhost:5173` и `localhost:3000`.
- CORS использует `AllowCredentials()`. Cookie требует этот режим.
- В Docker `Cors__AllowedOrigins__0` равен `PUBLIC_URL`.

## ExceptionHandlingMiddleware

Не меняйте формат ответа. Контент-тайп — `application/problem+json`.

- `BusinessException` → 400, `exception.Message`.
- `NotFoundException` → 404, `exception.Message`.
- `JsonException`/`BadHttpRequestException` → 400.
- Иное → 500 и `logger.LogError`.

Логируйте исключение один раз. Не дублируйте `LogError`.

## Конфигурация

- `appsettings.Development.json` в `.gitignore`: строка БД и `Jwt:Key`.
- Не коммитьте секреты и не выводите их в логи.
- OpenAPI и Scalar доступны только в Development.
- Контракт API сверяйте с `README.md` §4 и `TZ.md` §5.

Секреты: `../security.md`. Форматы: `../api.md`.
