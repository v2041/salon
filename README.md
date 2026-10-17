# Salon — система управления салоном красоты

Онлайн-запись клиентов по номеру телефона, витрина услуг, расписание мастера и аналитика дохода.
Салон на одного мастера: клиент записывается сам, мастер подтверждает и ведёт учёт.

- `backend/` — .NET 10, Minimal API, PostgreSQL + EF Core, FluentValidation, JWT в HttpOnly-cookie
- `frontend/salon/` — Next.js 16 (App Router), React 19, TypeScript, MUI 9, mobile-first
- Подробнее про задумку и план: [`TZ.md`](./TZ.md)

## 1. Возможности

**Клиент:**

- витрина услуг с группировкой по категориям
- flow записи: услуги (можно несколько) → дата → время → подтверждение
- «Мои записи» со статусами, отмена своей записи
- вход/регистрация по телефону + SMS-код (cookie `some-cookies`, 30 дней)

**Мастер (Admin):**

- CRUD услуг: цена, длительность, название, описание, категория, деактивация
- расписание: рабочие дни, перерывы, выходные
- записи: подтвердить / отклонить / завершить / пропуск / отменить, перенос времени, правка цены
- аналитика: выручка, средний чек, загрузка, популярность услуг, no-show, топ клиентов

**Система:**

- автогенерация расписания на 14 дней вперёд (10:00–18:00, перерыв 15:00–15:20, воскресенье — выходной)
- доменные проверки: без записи в прошлом, без пересечений, без записи в перерыв
- единый формат ошибок `application/problem+json` на русском

## 2. Структура репозитория

```
salon_project/
  README.md                # этот файл
  TZ.md                    # техзадание v1.0
  backend/
    backend.sln
    Domain/                # Entities, ValueObjects, Enums, Services, Exceptions
    Features/              # Appointments, Offerings, Schedules, Users, Analytics
    Infrastructure/        # SalonDbContext, Configurations, Migrations, Jwt, ScheduleService
    WebApi/                # Program.cs, Middlewares, appsettings
    DomainTests/           # xUnit-тесты домена
  frontend/
    salon/                 # Next.js-приложение
      app/(client)/booking | offerings | my-appointments
      components/booking/  # OfferingList, Calendar, TimeGrid, BookingSummaryBar
      components/          # MyAppointmentsList, BottomNavigationBar
      lib/format.ts        # форматирование цен, дат, длительностей (ru-RU)
```

Архитектура backend — DDD + vertical slices: правила в `Domain.Entities`
(`Schedule.AddAppointment`, `Appointment.Cancel/Confirm/...`), `Features/*` — HTTP + EF.

## 3. Быстрый старт

Требования: .NET SDK 10, PostgreSQL 15+, Node.js 20+, npm.

### Backend

```powershell
# 1. Создать БД
psql -U postgres -c "CREATE DATABASE salon;"

# 2. Настроить backend/WebApi/appsettings.Development.json (в .gitignore):
# {
#   "ConnectionStrings": { "SalonDbContext": "User ID=postgres;Password=1234;Host=localhost;Port=5432;Database=salon;" },
#   "Jwt": { "Issuer": "SalonApi", "Audience": "SalonApi", "Key": "<base64-ключ-32+байт>", "ExpiryMinutes": 60 }
# }

cd C:\Projects\salon_project\backend
dotnet restore
dotnet ef database update --project Infrastructure --startup-project WebApi
dotnet run --project WebApi
# API: http://localhost:5164, Scalar UI: http://localhost:5164/scalar/v1 (Development)
```

### Frontend

```powershell
cd C:\Projects\salon_project\frontend\salon
npm install
npm run dev
# http://localhost:3000 → /offerings, /booking, /my-appointments
```

> Сейчас frontend работает на мок-данных (`booking/page.tsx`, `MyAppointmentsList.tsx`).
> Для живого режима нужен `lib/api.ts` с `fetch(..., { credentials: "include" })` к `http://localhost:5164`.

### Тесты

```powershell
cd C:\Projects\salon_project\backend
dotnet test
# OfferingTests, ScheduleTests, MoneyTests, TimeIntervalTests
```

## 4. API

База: `http://localhost:5164`. Даты `yyyy-MM-dd`, время `HH:mm`, деньги `{ "value": 3500, "currency": "RUB" }`.
Статусы записи: `0=Pending, 1=Confirmed, 2=Completed, 3=Canceled, 4=Missed, 5=Rejected`.

### Auth — `api/auth`

| Метод | Путь | Тело / ответ |
|---|---|---|
| POST | `/send-code` | `{ "phone": "+79991234567" }` → `{ "expiresAt": "..." }`, код 4 цифры на 5 мин |
| POST | `/register` | `{ "code", "phone", "firstName", "lastName" }` → cookie `some-cookies` |
| POST | `/login` | `{ "code", "phone" }` → cookie `some-cookies` |

### Offerings — `/api/offerings`

| Метод | Путь | Примечание |
|---|---|---|
| GET | `/api/offerings/` | все активные, публично |
| GET | `/api/offerings/{id}` | одна услуга |
| POST | `/api/offerings/` | `{ "price": 1500, "title", "description", "duration": "01:00:00", "category": 0 }` |
| PUT | `/api/offerings/{id}` | частичное обновление, требует auth |
| DELETE | `/api/offerings/{id}` | `IsActive=false`, требует auth |

`Category: 0=Hair, 1=Nails, 2=Face, 3=Other`.

### Schedules — `/api/schedules`

| Метод | Путь | Примечание |
|---|---|---|
| GET | `/free-time?date=2026-08-10` | `{ "freeIntervals": [{ "start": "10:00", "end": "12:00" }] }` через `TimelineBuilder` |
| GET | `/available-dates?offeringIds=guid1,guid2` | `{ "dates": [...] }` — дни с окном ≥ суммарной длительности |
| GET | `/` | будущие дни |
| POST | `/` | `{ "date", "workInterval": {"start","end"}, "breakInterval?": {...} }` |
| PUT | `/` | `{ "id", "isWorking", "workInterval", "breakInterval?" }` |

### Appointments — `/api/appointments`

| Метод | Путь | Примечание |
|---|---|---|
| GET | `/{id}` | одна запись (Admin) |
| GET | `/all` | все записи: `userName`, `userPhone`, `offerings[{offeringId,title,price,duration}]` (Admin) |
| GET | `/user` | записи текущего пользователя |
| POST | `/` | `{ "date", "time", "offeringIds": [...] }` → `201 { "id", "date", "interval", "status", "price" }`; `userId` — из JWT |
| POST | `/admin` | `{ "phone", "date", "time", "offeringIds" }` — мастер создаёт запись клиенту (Admin) |
| PATCH | `/{id}/cancel` | отмена своей записи (User) |
| PATCH | `/{id}/change-status` | `{ "id", "status": 0..5 }` → `204` (Admin) |
| PATCH | `/{id}/change-time` | `{ "id", "date", "time" }` — перенос (Admin) |
| PATCH | `/{AppointmentId}/offering/{OfferingId}/change-price` | `{ "value", "currency" }` (Admin) |
| PATCH | `/{AppointmentId}/offering/{OfferingId}/change-duration` | `"01:30:00"` — мин. 10 минут (Admin) |

### Analytics — `api/analytics`

`GET /gain?from=&to=` — выручка по `Completed` за период.
Также: `/offerings-popularity`, `/no-show-rate`, `/average-check`, `/workload-rate`, `/best-clients`.

### Ошибки

`ExceptionHandlingMiddleware` → `application/problem+json`:
`BusinessException → 400`, `NotFoundException → 404`, `Json/BadRequest → 400`, остальное → `500`.

## 5. Домен и конфигурация

- `User{Phone(unique), Role: Default|Admin}` — сиды: Admin `+79991234567`, Client `+79341234567`
- `Offering{Price:Money, Title(≤50), Duration(>10 мин), IsActive, Category}`
- `Schedule{Date(unique), IsWorking, WorkInterval(≥2ч), BreakInterval?(строго внутри, не впритык)}`
- `Appointment{Interval, Status, Offerings[]}` + снимок цены/длительности в `AppointmentOffering`
- Нельзя: запись в прошлом, вне рабочего времени, в перерыв, с пересечением `Pending|Confirmed`
- `Cancel` из `Confirmed` — не позже чем за 2 часа; закрыть день — только без активных записей

| Где | Ключ | Зачем |
|---|---|---|
| `appsettings.Development.json` | `ConnectionStrings:SalonDbContext` | строка Postgres (файл в `.gitignore`) |
| | `Jwt:Issuer/Audience/Key/ExpiryMinutes` | подпись токена, сейчас 60 мин |
| `Program.cs` | CORS `localhost:5173, localhost:3000` | для фронта; для cookie нужен `AllowCredentials()` |
| cookie | `some-cookies`, `HttpOnly`, 30 дней | имя и TTL расходятся с JWT — выровнять в проде |

## 6. Деплой

Минимум 1 vCPU / 2 GB RAM, Ubuntu 22.04 / Debian 12:

```powershell
dotnet publish backend/WebApi -c Release -o ./publish
dotnet ef database update --project backend/Infrastructure --startup-project backend/WebApi
# systemd unit salon-api.service → dotnet /opt/salon/WebApi.dll
```

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80 443
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish backend/WebApi -c Release -o /app/publish
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WebApi.dll"]
```

## 7. Статус и дальше

Готово: домен + CRUD услуг/расписания/записей, auth по коду, gain-аналитика,
фоновый `ScheduleService`, доменные тесты, клиентские экраны на моках.

Дальше по `TZ.md`: починить префиксы `/api/auth`, `/api/analytics` и `RequireAdmin`,
закрыть авторизацию (`userId` из JWT), подключить фронт к API, панель мастера,
summary-аналитика + графики, rate-limit, `/healthz`, CI/CD.

Лицензия MIT. Контакт: makstara55@gmail.com, github.com/v2041.



