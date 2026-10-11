---
name: add-feature
description: Use when the user asks to add a new API endpoint, a new feature/slice, a new business rule, a new CRUD operation, or says "добавь эндпоинт", "новая фича", "новый endpoint", "сделай API для". Covers the full vertical-slice path: Domain → DomainTests → Features → module registration → build check.
---

# Add feature (vertical slice)

Реализуй фичу по слоям. Порядок обязателен: сначала Domain и тесты, потом HTTP.

## Шаг 1 — пойми требование

1. Найди ID требования в `TZ.md` §3 (FR-*) или §5 (API). Упомяни его в описании работы.
2. Определи сущность: Offering, Schedule, Appointment, User.
3. Прочитай локальные инструкции слоя:
   - `backend/Domain/AGENTS.md`
   - `backend/Features/AGENTS.md`
   - `backend/DomainTests/AGENTS.md`

## Шаг 2 — Domain + тесты (до любого HTTP-кода)

1. Новое бизнес-правило → метод сущности или доменный сервис в `backend/Domain/`.
   Проверка — через `throw new BusinessException("...на русском...")`.
   Пример: `schedule.AddAppointment(...)`, `appointment.Cancel(now)`.
2. Сразу тесты в `backend/DomainTests/`:
   - happy path + нарушение правила;
   - naming `Method_Condition_Expectation`, вложенный класс по методу;
   - только `Assert.*`, без FluentAssertions.
3. Проверка:

```powershell
dotnet test
```

работает из `backend/`. Тесты красные — не иди дальше.

## Шаг 3 — Features-слайс

Создай `backend/Features/<Сущность>/<ИмяФичи>/`:

| Файл | Содержимое |
|---|---|
| `<Имя>Request.cs` | `record` с полями входа. В теле НЕТ `userId`/`phone` — они из JWT claims |
| `<Имя>Response.cs` | `record` с тем, что уйдёт клиенту |
| `Endpoint.cs` | `public static class Endpoint`, метод `...Async` с DI-параметрами |
| `Validator.cs` | FluentValidation, `CascadeMode.Stop`, сообщения на русском (если нужна валидация) |

Порядок внутри Endpoint: валидация → вызов доменного метода → `db.Add` → `SaveChanges` → `Results.Created/Ok/NoContent`.

Верни клиенту форматы проекта: даты `yyyy-MM-dd`, время `HH:mm`, деньги `{ "value", "currency" }`.

## Шаг 4 — регистрация роутинга

1. Добавь маршрут в `Features/<Сущность>/<Сущность>Module.cs`:

```csharp
group.MapPost("/", NewFeature.Endpoint.NewFeatureAsync).RequireAuthorization("Admin");
```

2. Политики: управляющие — `"Admin"`, клиентские — `"User"`, публичные — без `RequireAuthorization`.
3. Если фича новой сущности — создай модуль и вызови `app.Map<X>Endpoints();` в `WebApi/Program.cs`.

## Шаг 5 — документация (обязательно)

1. Из `backend/`:

```powershell
dotnet build
dotnet test
```

2. `README.md` §4 — одна строка в таблицу API: метод, путь, примечание.
3. `TZ.md` — обнови статус под реализованное:
   - §2 «Текущее состояние»: перенеси endpoint из «должно быть» в «есть», убери «нет (должна быть Admin)» → поставь фактическую авторизацию;
   - §5 «Добавить»: выполненное удали из списка или пометь как сделанное;
   - если requirement из §3 закрыт целиком — отметь в его строке таблицы.
4. Если менялись стек/команды — `AGENTS.md` слоя.

## Шаг 6 — независимое ревью (субагент reviewer)

Вызови субагента `reviewer` (Task tool, subagent_type: "reviewer") с заданием:
«Проведи ревью незакоммиченных изменений». Он работает только на чтение.

- NEEDS FIXES → исправь критичное и важное, затем повтори `dotnet build` + `dotnet test`.
- APPROVE → следующий шаг.

## Шаг 7 — коммит (субагент committer)

Всегда по завершении задачи, без отдельного запроса. Вызови субагента `committer`
(Task tool, subagent_type: "committer") с кратким описанием задачи и FR-ID.
Он сам сделает staging (без секретов и мусора) и напишет сообщение.

## Частые ошибки — не допускать

- `userId` из тела запроса (нарушен FR-S2) — только из claims.
- Бизнес-проверка в Endpoint вместо Domain.
- Миграция без тестов домена.
- Забыт `CancellationToken` в endpoint.
- Готовая, но незарегистрированная фича (`ChangeAppointmentTime`, `ChangeOffering`) — сначала grep, вдруг уже написана: подключи, не дублируй.
