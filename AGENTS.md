# AGENTS.md — инструкции для AI-агентов

Файл читается агентом (opencode и др.) перед каждой задачей в этом репозитории.
Обновляйте его, когда меняются стек, команды или конвенции.

## Что за проект

Салон красоты (1 мастер): онлайн-запись клиентов, витрина услуг, расписание, аналитика.
Детали продукта: `TZ.md` (техзадание), `README.md` (API и быстрый старт).

- `backend/` — .NET 10, Minimal API, PostgreSQL + EF Core, FluentValidation, JWT в HttpOnly-cookie, xUnit
- `frontend/salon/` — Next.js 16 (App Router), React 19, TypeScript, MUI 9, mobile-first

## Локальные инструкции (читай при работе в директории)

Перед правками в слое сверяйся с его AGENTS.md — там точные конвенции с примерами:

| Файл | Когда читать |
|---|---|
| `backend/Domain/AGENTS.md` | сущности, Value Objects, доменные методы, исключения |
| `backend/Features/AGENTS.md` | эндпоинты, DTO, валидаторы, модули роутинга |
| `backend/DomainTests/AGENTS.md` | xUnit-тесты домена, naming, Arrange/Assert |
| `backend/Infrastructure/AGENTS.md` | EF-конфигурации, миграции, JWT, фоновые сервисы |
| `backend/WebApi/AGENTS.md` | Program.cs, middleware, auth-политики, CORS |
| `frontend/salon/AGENTS.md` | React/Next.js компоненты, MUI sx, моки, форматы |

## Команды

```powershell
# Backend: сборка и тесты (выполнять из backend/)
dotnet build
dotnet test            # доменные тесты: Offering, Schedule, Money, TimeInterval

# Миграции (нужен запущенный PostgreSQL и appsettings.Development.json)
dotnet ef database update --project Infrastructure --startup-project WebApi

# Запуск API: http://localhost:5164
dotnet run --project WebApi

# Frontend: выполнять из frontend/salon/
npm install
npm run lint           # ESLint
npm run build          # проверка типов и сборки Next.js
npm run dev            # http://localhost:3000
```

**Проверка после любых правок:** `dotnet build` + `dotnet test` для backend,
`npm run lint` для frontend. Ничего не коммитить без явной просьбы.

## Архитектура backend (DDD + vertical slices)

- `Domain/` — сущности, Value Objects, enum'ы, доменные сервисы, исключения.
  Бизнес-правила живут ТОЛЬКО здесь: `Schedule.AddAppointment`, `Appointment.Cancel/Confirm/...`.
  Новая логика — сначала в Domain + тест в `DomainTests/`, потом HTTP-слой.
- `Features/` — по папке на каждый сценарий (Appointments, Offerings, Schedules, Users, Analytics):
  request/response DTO, валидация FluentValidation, обработчик, регистрация роутера.
  Незарегистрированные features: `ChangeAppointmentTime`, `ChangeAppointmentOfferingPrice`,
  `ChangeOffering` — код есть, роутер не подключён (см. TZ §5).
- `Infrastructure/` — `SalonDbContext`, конфигурации EF, миграции, JWT, фоновый `ScheduleService`.
- `WebApi/` — `Program.cs`, middleware, `appsettings`.

## Конвенции и форматы

- Даты `yyyy-MM-dd`, время `HH:mm`, деньги `{ "value": 3500, "currency": "RUB" }`.
- Статусы записи: `0=Pending, 1=Confirmed, 2=Completed, 3=Canceled, 4=Missed, 5=Rejected`.
- Категории услуг: `0=Hair, 1=Nails, 2=Face, 3=Other`.
- Ошибки: `ExceptionHandlingMiddleware` → `application/problem+json`, сообщения на русском.
  `BusinessException` → 4xx, `NotFoundException` → 404. Не логировать исключения дважды.
- Авторизация: `userId`/`phone` берутся из JWT-claims, НЕ из тела запроса (FR-S2).
- Мутации домена — только через доменные методы, не напрямую через EF.

## Frontend

- Всё ещё на мок-данных (`booking/page.tsx`, `MyAppointmentsList.tsx`).
  Живой режим: `lib/api.ts` с `fetch(..., { credentials: "include" })` → `http://localhost:5164`.
- Роуты в `app/(client)/`: `offerings`, `booking`, `my-appointments`.
- Общие форматы — `lib/format.ts` (ru-RU). UI mobile-first на MUI.
- Не трогать `node_modules/`, `bin/`, `obj/`, `.next/` (в .gitignore).

## Секреты и конфигурация

- `backend/WebApi/appsettings.Development.json` в `.gitignore` — содержит строку БД и JWT-ключ.
  Никогда не коммитить и не вставлять секреты в код/логи.
- Тестовый доступ: Admin `+79991234567`, Client `+79341234567` (сиды).

## Процесс разработки (субагенты)

Последовательность для любой фичи (детали — скилл `add-feature`):
реализация → `dotnet build/test` → обновить `README.md` §4 и `TZ.md` → ревью → коммит.

- **reviewer** (`.opencode/agents/reviewer.md`) — независимое ревью незакоммиченных
  изменений: конвенции слоёв, Domain-первичность, FR-S2, секреты, TZ/README.
  Только чтение. NEEDS FIXES → исправить и пересобрать.
- **committer** (`.opencode/agents/committer.md`) — staging и commit ТОЛЬКО по явной
  просьбе пользователя. Без секретов (`appsettings.Development.json`), без push/amend.

## Актуальность TZ.md и README.md

- Каждая реализованная фича обязана обновить `README.md` §4 (таблица API).
- `TZ.md` — живой документ: закрытые требования (§2, §5) переносить в «есть/сделано»,
  не оставлять ложь. Новые требования — добавлять сюда до кода.

## Полезные указания агенту

- Перед изменением API сверяйтесь с таблицами в `README.md` §4 и `TZ.md` §2.1, §5.
- Новые требования берите из `TZ.md` §3 (ID вроде FR-B1, FR-S1) — упоминайте ID в описании работы.
- Миграции EF: не редактировать созданные, добавлять новую через
  `dotnet ef migrations add <Name> --project Infrastructure --startup-project WebApi`.
- Если меняется стек/команды — обновите этот файл.
