# Salon — система управления салоном красоты

![Language: C#](https://img.shields.io/badge/language-C%23-informational)
![Language: TypeScript](https://img.shields.io/badge/language-TypeScript-informational)
![Version](https://img.shields.io/badge/version-0.1.0-brightgreen)

Онлайн-запись в салон красоты на одного мастера. Клиент записывается сам по номеру телефона, мастер ведёт расписание, услуги и аналитику дохода.

- `backend/` — .NET 10 (Minimal API), DDD и vertical slices, PostgreSQL + EF Core, FluentValidation, JWT в HttpOnly-cookie.
- `frontend/salon/` — Next.js 16 (App Router), React 19, TypeScript, MUI 9. Интерфейс mobile-first.

Продуктовые требования и план — в [TZ.md](./TZ.md).

## Возможности

**Клиент**

- поток записи: услуги (можно несколько) → дата → время → подтверждение. Услуги можно выбрать в потоке или на странице услуги `/offerings/{id}`;
- «Мои записи»: статусы, отмена своей записи в статусах `Pending` и `Confirmed` не позднее чем за 2 часа;
- вход и регистрация по телефону. Код — 4 цифры, действует 5 минут. Повторная отправка заменяет старый код. Лимиты: 5 кодов в час и 20 попыток в час. Пять неверных попыток блокируют код. SMS-провайдер не подключён; в `Development` код приходит в ответе `send-code`.

**Мастер**

- CRUD услуг: цена, длительность, название, описание, категория, деактивация и восстановление;
- расписание: рабочие дни, перерывы, выходные; день показан вместе с записями;
- записи: подтверждение, отклонение, завершение, пропуск, отмена, перенос времени, правка цены и длительности; фильтры по периоду и статусу;
- аналитика: выручка (всего и по месяцам), средний чек, загрузка, популярность услуг, no-show, отмены/отказы/пропуски, клиенты (всего/новые/повторные), топ клиентов.

**Система**

- автогенерация расписания на 14 дней вперёд (10:00–18:00, перерыв 15:00–15:20, воскресенье — выходной);
- доменные проверки: нет записи в прошлом, нет пересечений, нет записи в перерыв и выходной день;
- защита от гонок: уникальный индекс по слоту, конфликт даёт `400`;
- `GET /healthz` проверяет подключение к БД и отдаёт `200` или `503`;
- единый формат ошибок `application/problem+json` на русском языке.

## Структура репозитория

```
salon_project/
  backend/
    backend.sln
    Domain/           # Entities, ValueObjects, Enums, Services, Exceptions — бизнес-правила
    Features/         # Appointments, Offerings, Schedules, Users, Analytics (vertical slices)
    Infrastructure/   # SalonDbContext, EF-конфигурации, миграции, JWT, ScheduleService
    WebApi/           # Program.cs, Middlewares, appsettings
    Seeder/           # демо-данные для Docker-стенда
    DomainTests/      # xUnit-тесты домена
    WebApiTests/      # интеграционные HTTP-тесты (WebApplicationFactory + Testcontainers)
  frontend/
    salon/            # Next.js-приложение
      app/(app)/      # booking, my-appointments, admin, offerings/[id]
      components/     # booking/*, admin/*, LoginScreen, MyAppointmentsList, ThemeRegistry
      lib/            # api.ts, auth.tsx, format.ts, slots.ts, types.ts, theme.ts, useBookingFlow.ts
  TZ.md               # техзадание
```

Бизнес-правила живут только в `Domain` (`Schedule.AddAppointment`, `Appointment.Cancel/Confirm/...`). Слой `Features` — HTTP и сохранение через EF.

## Требования

- .NET SDK 10;
- PostgreSQL 15+;
- Node.js 24 (проверено на 24.18.0) и npm;
- Docker Desktop + WSL2 — только для запуска через `docker compose`.

## Установка и запуск

### 1. База данных и настройки backend

Создайте базу:

```powershell
psql -U postgres -c "CREATE DATABASE salon;"
```

Создайте `backend/WebApi/appsettings.Development.json` (файл в `.gitignore`):

```json
{
  "ConnectionStrings": { "SalonDbContext": "User ID=postgres;Password=<пароль>;Host=localhost;Port=5432;Database=salon;" },
  "Jwt": { "Issuer": "SalonApi", "Audience": "SalonApi", "Key": "<строка не короче 32 символов>", "ExpiryMinutes": 43200 }
}
```

### 2. Backend

```powershell
cd backend
dotnet restore
dotnet ef database update --project Infrastructure --startup-project WebApi
dotnet run --project WebApi
```

- API: http://localhost:5164
- Scalar UI (только Development): http://localhost:5164/scalar/v1

### 3. Frontend

```powershell
cd frontend/salon
npm install
npm run dev
```

http://localhost:3000 → `/booking`, `/my-appointments`, `/admin`. Все экраны работают с живым API (`lib/api.ts`, `fetch` с `credentials: "include"`). Мок-данных нет.

## Запуск в Docker

Стек: PostgreSQL 17, API (.NET 10), фронтенд (Node 24), Caddy (reverse proxy, HTTPS).

1. Скопируйте `.env.example` в `.env` и задайте значения:
   - `POSTGRES_PASSWORD` — пароль БД;
   - `JWT_KEY` — случайная строка не короче 32 символов (`openssl rand -base64 48`);
   - `PUBLIC_URL` — публичный адрес сайта: `https://salon.example.com` или `http://1.2.3.4`;
   - `DOMAIN` — домен для Caddy: `salon.example.com` (авто-HTTPS) или `:80` (проверка по IP без TLS).

2. Соберите и запустите:

```powershell
docker compose up --build -d
```

3. Миграции применяет сервис `migrate` перед стартом API. Сайт открывается по `PUBLIC_URL`.

| Команда | Действие |
|---|---|
| `docker compose logs -f api` | Логи API |
| `docker compose ps` | Состояние сервисов |
| `docker compose down` | Остановить стек |
| `docker compose up --build -d` | Пересобрать и обновить |
| `docker compose exec db psql -U salon -d salon` | Консоль БД |

Данные БД — volume `pgdata`, сертификаты Caddy — `caddy_data`. После смены `PUBLIC_URL` пересоберите фронтенд: `docker compose build frontend`.

### Демо-данные для проверки

Сид `seed` создает демо-данные. Он добавляет 60 клиентов, 22 услуги (19 активных) и 142 дня расписания. Всего около 370 записей: 120 дней назад и 21 день вперёд.

1. Остановите API, чтобы фоновый `ScheduleService` не менял расписание:
```powershell
docker compose stop api
```
2. Запустите сид:
```powershell
docker compose --profile demo run --rm --build seed
```
3. Запустите API:
```powershell
docker compose start api
```

Сид удаляет записи, услуги, расписания, клиентов и коды подтверждения. Тестовые Admin `+79991234567` и Client `+79341234567` остаются. Код входа — в таблице `VerificationCodes`. Сервис `seed` задает `ALLOW_DEMO_SEED=true` и флаг `--reset`. Без флага `--reset` сид останавливается, если в БД уже есть данные.

## Скрипты frontend

| Команда | Действие |
|---|---|
| `npm run dev` | Dev-сервер `next dev` (порт 3000) |
| `npm run build` | Продакшен-сборка и проверка типов |
| `npm run start` | Запуск собранного приложения |
| `npm run lint` | ESLint |

## Тесты

```powershell
cd backend
dotnet test
```

- `DomainTests` — xUnit-тесты домена: `Offering`, `Schedule`, `Money`, `TimeInterval`, `Appointment`, `User`, `TimelineBuilder`, `VerificationCode`.
- `WebApiTests` — интеграционные HTTP-тесты на `WebApplicationFactory` и PostgreSQL через Testcontainers. Для запуска нужен Docker.

## API

База: `http://localhost:5164`.

- Даты — `yyyy-MM-dd`, время — `HH:mm` (в ответах `HH:mm:ss`), деньги — `{ "value": 3500, "currency": "RUB" }`.
- Статусы записи: `0=Pending, 1=Confirmed, 2=Completed, 3=Canceled, 4=Missed, 5=Rejected`.
- Категории услуг: `0=Hair, 1=Nails, 2=Face, 3=Other`.
- Аутентификация: JWT в HttpOnly-cookie `some-cookies` (30 дней). `userId` берётся из claims, не из тела запроса.

### Auth — `api/auth`

| Метод | Путь | Тело / ответ |
|---|---|---|
| POST | `/send-code` | `{ "phone": "+79991234567" }` → `{ "expiresAt", "isRegistered", "code"? }` |
| POST | `/register` | `{ "code", "phone", "firstName", "lastName" }` → cookie `some-cookies` |
| POST | `/login` | `{ "code", "phone" }` → cookie `some-cookies` |
| POST | `/logout` | удаляет cookie `some-cookies` |
| GET | `/me` | текущий пользователь |

Поле `code` приходит только в `Development`. Повторная отправка заменяет старый код.
Неверный или истёкший код → `400`. Пять неверных попыток блокируют код.
Лимиты: 5 кодов в час и 20 попыток в час → `429`.
Телефон из `AdminPhone` получает роль Admin при регистрации.

### Offerings — `/api/offerings`

| Метод | Путь | Примечание |
|---|---|---|
| GET | `/api/offerings/` | все активные, публично |
| GET | `/api/offerings/all` | все услуги, включая неактивные (Admin) |
| GET | `/api/offerings/{id}` | одна активная услуга; неактивная или неизвестная → `404` |
| POST | `/api/offerings/` | `{ "price": 1500, "title", "description", "duration": "01:00:00", "category": 0 }`, Admin |
| PUT | `/api/offerings/{id}` | обновление (передаются все поля), Admin |
| DELETE | `/api/offerings/{id}` | `IsActive=false`, Admin |
| PATCH | `/api/offerings/{id}/restore` | `IsActive=true`, Admin |

### Schedules — `/api/schedules`

| Метод | Путь | Примечание |
|---|---|---|
| GET | `/free-time?date=2026-08-10` | свободные интервалы дня: `{ "freeIntervals": [...] }` |
| GET | `/available-dates?offeringIds=guid1,guid2` | дни с окном не меньше суммарной длительности |
| GET | `/` | будущие дни с `isWorking` и записями (Admin) |
| POST | `/` | `{ "date", "workInterval": { "start", "end" }, "breakInterval?": {...} }`; 409, если день уже есть |
| PUT | `/` | `{ "id", "isWorking", "workInterval", "breakInterval?" }` |

### Appointments — `/api/appointments`

| Метод | Путь | Примечание |
|---|---|---|
| GET | `/{id}` | одна запись (Admin) |
| GET | `/all` | все записи с клиентом и услугами (Admin) |
| GET | `/user` | записи текущего пользователя |
| POST | `/` | `{ "date", "time", "offeringIds": [...] }` → `201`; `userId` — из JWT |
| POST | `/admin` | `{ "phone", "firstName?", "lastName?", "date", "time", "offeringIds" }` — запись клиента мастером |
| PATCH | `/{id}/cancel` | отмена своей записи (User) |
| PATCH | `/{id}/change-status` | `{ "id", "status": 0..5 }` → `204` (Admin) |
| PATCH | `/{id}/change-time` | `{ "id", "date", "time" }` — перенос (Admin) |
| PATCH | `/{AppointmentId}/offering/{OfferingId}/change-price` | `{ "value", "currency" }` (Admin) |
| PATCH | `/{AppointmentId}/offering/{OfferingId}/change-duration` | `"01:30:00"` — дольше 10 минут; пересчитывает интервал записи (Admin) |

### Analytics — `api/analytics`

Все эндпоинты требуют роль Admin.

| Путь | Примечание |
|---|---|
| `/gain?from=&to=` | выручка по `Completed` за период |
| `/gain-by-month?from=&to=` | выручка по месяцам (N1) |
| `/offerings-gain` | выручка по услугам (N1) |
| `/offerings-popularity` | популярность услуг |
| `/average-check` | средний чек по записям |
| `/average-hour-gain` | средняя выручка в час |
| `/workload-rate` | загрузка |
| `/no-show-rate` | доля пропусков; пустой период → `0` |
| `/clients?from=&to=` | всего, новые и повторные клиенты (N2) |
| `/losses?from=&to=` | отмены, отказы, пропуски — количество и процент (N4) |
| `/best-clients?limit=` | топ клиентов; `limit ≥ 1` |

### Ошибки

`ExceptionHandlingMiddleware` → `application/problem+json`:
`BusinessException → 400`, `NotFoundException → 404`, `RateLimitException → 429`,
`Json/BadRequest → 400`, остальное → `500` без деталей.
Ответы без тела (`401`, `403`, `404`, `429`) тоже получают `application/problem+json`.
Служебный эндпоинт `GET /healthz` не требует авторизации и отдаёт `200` или `503`.

## Домен и правила

- `User{Phone(unique), Role: Default|Admin}`; тестовые доступы: Admin `+79991234567`, Client `+79341234567`.
- `Offering{Price: Money, Title(≤50), Duration(>10 мин), IsActive, Category}`.
- `Schedule{Date(unique), IsWorking, WorkInterval(≥2ч), BreakInterval?(строго внутри, не впритык)}`.
- `Appointment{Interval, Status, Offerings[], CreatedAt, CompletedAt?}`; цена и длительность фиксируются в `AppointmentOffering` на момент записи.
- Нельзя: запись в прошлом, вне рабочего времени, в перерыв, в выходной день, с пересечением `Pending|Confirmed`.
- Отмена из `Pending` и `Confirmed` — не позже чем за 2 часа. Закрыть день можно только без активных записей.

| Где | Ключ | Зачем |
|---|---|---|
| `appsettings.Development.json` | `ConnectionStrings:SalonDbContext` | строка подключения PostgreSQL (файл в `.gitignore`) |
| `appsettings.Development.json` | `Jwt:Issuer`, `Jwt:Audience`, `Jwt:Key`, `Jwt:ExpiryMinutes` | параметры токена; срок 30 дней, как у cookie |
| `appsettings.json` | `AdminPhone` | телефон, который получает роль Admin при регистрации |
| `WebApi/Program.cs` | CORS: `localhost:5173`, `localhost:3000` | для фронтенда, `AllowCredentials()` включён |
| cookie | `some-cookies`, `HttpOnly`, 30 дней | сессия клиента и мастера |

## Продакшен

```powershell
dotnet publish backend/WebApi -c Release -o ./publish
dotnet ef database update --project backend/Infrastructure --startup-project backend/WebApi
```

## Лицензия

TODO: файл `LICENSE` в репозитории отсутствует — добавьте его, чтобы закрепить лицензию.

Автор: [v2041](https://github.com/v2041) · makstara55@gmail.com
