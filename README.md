# Salon — онлайн-запись в салон красоты

[![CI](https://github.com/v2041/salon/actions/workflows/ci.yml/badge.svg)](https://github.com/v2041/salon/actions/workflows/ci.yml)
![.NET](https://img.shields.io/badge/.NET-10-512BD4)
![Next.js](https://img.shields.io/badge/Next.js-16-000000)
![License: MIT](https://img.shields.io/badge/license-MIT-green)

Веб-система для салона красоты на одного мастера. Клиент записывается онлайн по номеру телефона. Мастер ведёт услуги, расписание и аналитику дохода.

- `backend/` — .NET 10, Minimal API, DDD и vertical slices, PostgreSQL + EF Core, FluentValidation, JWT в HttpOnly-cookie.
- `frontend/salon/` — Next.js 16 (App Router), React 19, TypeScript, MUI 9. Интерфейс mobile-first.

Документы: [TZ.md](./TZ.md) — требования и план. [docs/README.md](./docs/README.md) — карта документации. [docs/openapi.json](./docs/openapi.json) — контракт API.

## Возможности

**Клиент**

- Поток записи: услуги (можно несколько) → дата → время → подтверждение. Список услуг встроен в поток записи, отдельной витрины нет.
- «Мои записи»: статусы и отмена своей записи.
- Вход по телефону. Код — 4 цифры, срок действия 5 минут. Повторная отправка — не чаще 1 раза в минуту. Пять неверных попыток блокируют код.
- Код приходит в Telegram-бот салона. Клиент один раз привязывает номер кнопкой «Поделиться номером». В dev-режиме без токена бота код виден на экране входа.

**Мастер**

- Услуги: создание, изменение, деактивация. Поля: цена, длительность, название, описание, категория.
- Расписание: рабочие дни, перерывы, выходные.
- Записи: подтверждение, отклонение, завершение, пропуск, отмена, перенос времени, правка цены и длительности.
- Аналитика: выручка, средний чек, загрузка, популярность услуг, no-show, топ клиентов.

**Система**

- Фоновый сервис `ScheduleService` создаёт расписание по настройкам мастера: рабочие часы, перерыв, выходные дни и горизонт генерации.
- Доменные проверки: нет записи в прошлом, в перерыв, в выходной день и с пересечением.
- Ошибки в формате `application/problem+json`, сообщения на русском языке.

## Структура репозитория

```text
salon_project/
├── backend/
│   ├── Domain/           # сущности, Value Objects, правила
│   ├── Features/         # сценарии: Appointments, Offerings, Schedules, Users, Analytics
│   ├── Infrastructure/   # DbContext, EF-конфигурации, миграции, JWT, Telegram, фоновые сервисы
│   ├── WebApi/           # Program.cs, middleware, appsettings
│   ├── Seeder/           # демо-данные для Docker
│   └── DomainTests/      # xUnit-тесты домена
├── frontend/salon/
│   ├── app/(app)/        # booking, my-appointments, admin
│   ├── components/       # booking/*, admin/*
│   └── lib/              # api.ts, auth.tsx, format.ts, slots.ts, types.ts, theme.ts
├── docs/                 # документация для разработки
├── TZ.md                 # техническое задание
└── docker-compose.yml    # PostgreSQL, API, frontend, Caddy
```

Бизнес-правила живут только в `Domain`. Слой `Features` — HTTP и сохранение через EF Core.

## Требования

- .NET SDK 10;
- PostgreSQL (в Docker Compose — 17);
- Node.js 24 и npm;
- Docker Desktop + WSL2 — только для запуска через `docker compose`.

## Быстрый старт

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

Токен бота не обязателен. Без токена Development использует fake-отправитель: код пишется в лог API и приходит в ответе `send-code`.

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

Откройте http://localhost:3000. Главная перенаправляет на `/booking`. Другие экраны: `/my-appointments`, `/admin`.

Frontend берёт адрес API из `NEXT_PUBLIC_API_URL`. По умолчанию — `http://localhost:5164`. Все экраны работают с живым API через `lib/api.ts` (`fetch` с `credentials: "include"`). Мок-данных нет.

| Команда | Действие |
|---|---|
| `npm run dev` | dev-сервер на порту 3000 |
| `npm run build` | продакшен-сборка и проверка типов |
| `npm run start` | запуск собранного приложения |
| `npm run lint` | ESLint |

## Docker Compose

Стек: PostgreSQL 17, API (.NET 10), frontend (Node 24), Caddy (reverse proxy, HTTPS).

1. Скопируйте `.env.example` в `.env`. Задайте значения:

| Переменная | Назначение |
|---|---|
| `POSTGRES_PASSWORD` | пароль БД |
| `JWT_KEY` | ключ JWT, не короче 32 символов (`openssl rand -base64 48`) |
| `PUBLIC_URL` | публичный адрес сайта: `https://salon.example.com` или `http://1.2.3.4` |
| `DOMAIN` | домен для Caddy: `salon.example.com` (авто-HTTPS) или `:80` (без TLS) |
| `TELEGRAM_BOT_TOKEN` | токен бота от @BotFather. Production без токена не стартует |
| `TELEGRAM_BOT_USERNAME` | имя бота без `@` для ссылки привязки |
| `TZ` | часовой пояс, по умолчанию `Europe/Moscow` |

2. Соберите и запустите:

```powershell
docker compose up --build -d
```

Сервис `migrate` применяет миграции перед стартом API. Сайт открывается по `PUBLIC_URL`.

| Команда | Действие |
|---|---|
| `docker compose logs -f api` | логи API |
| `docker compose ps` | состояние сервисов |
| `docker compose down` | остановить стек |
| `docker compose up --build -d` | пересобрать и обновить |
| `docker compose exec db psql -U salon -d salon` | консоль БД |

Данные БД лежат в volume `pgdata`, сертификаты Caddy — в `caddy_data`. После смены `PUBLIC_URL` пересоберите frontend: `docker compose build frontend`.

Минимальные требования VPS: 1 vCPU, 2 GB RAM.

### Демо-данные

Сид создаёт 60 клиентов, 22 услуги (19 активных), 142 дня расписания и около 370 записей.

1. Остановите API. Фоновый `ScheduleService` не должен менять расписание:

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

Сид удаляет записи, услуги, расписания, клиентов и коды подтверждения. Тестовые Admin `+79991234567` и Client `+79341234567` остаются. Сервис `seed` задаёт `ALLOW_DEMO_SEED=true` и флаг `--reset`. Без `--reset` сид останавливается, если в БД уже есть данные.

## API

База в dev: `http://localhost:5164`. Контракт: [docs/openapi.json](./docs/openapi.json). Форматы и правила: [docs/api.md](./docs/api.md).

- Даты — `yyyy-MM-dd`. Время в запросе — `HH:mm`, в ответе — `HH:mm:ss`. Деньги — `{ "value": 3500, "currency": "RUB" }`.
- Статусы записи: `0=Pending`, `1=Confirmed`, `2=Completed`, `3=Canceled`, `4=Missed`, `5=Rejected`.
- Категории услуг: `0=Hair`, `1=Nails`, `2=Face`, `3=Other`.
- JWT хранится в HttpOnly-cookie `some-cookies` (30 дней). `userId` берётся из claims, не из тела запроса.

### Auth — `/api/auth`

| Метод | Путь | Назначение |
|---|---|---|
| POST | `/send-code` | выдать код и отправить в Telegram; в dev-режиме код приходит в ответе |
| POST | `/login` | вход по телефону и коду → cookie |
| POST | `/register` | регистрация нового номера → cookie |
| POST | `/logout` | выход, удаляет cookie → 204 |
| GET | `/me` | текущий пользователь; без cookie → 401 |

`status: "sent"` — код отправлен. `status: "binding_required"` — номер не привязан: откройте `bindingUrl` и нажмите «Поделиться номером» в боте.

### Offerings — `/api/offerings`

| Метод | Путь | Доступ |
|---|---|---|
| GET | `/` | все активные услуги, публично |
| GET | `/{id}` | одна услуга, публично |
| POST | `/` | создать услугу, Admin |
| PUT | `/{id}` | изменить услугу, Admin |
| DELETE | `/{id}` | деактивировать услугу, Admin |

### Schedules — `/api/schedules`

| Метод | Путь | Назначение |
|---|---|---|
| GET | `/free-time?date=2026-08-10` | свободные окна дня, публично |
| GET | `/available-dates?offeringIds=guid1,guid2` | даты с доступным временем, публично |
| GET | `/` | будущие дни, Admin |
| POST | `/` | создать день, Admin |
| PUT | `/` | изменить день, Admin |

### Appointments — `/api/appointments`

| Метод | Путь | Назначение |
|---|---|---|
| GET | `/{id}` | одна запись, Admin |
| GET | `/all` | все записи, Admin |
| GET | `/user` | записи текущего клиента |
| POST | `/` | создать запись, `userId` из JWT |
| POST | `/admin` | запись клиента мастером, Admin |
| PATCH | `/{id}/cancel` | отмена своей записи |
| PATCH | `/{id}/change-status` | смена статуса, Admin |
| PATCH | `/{id}/change-time` | перенос времени, Admin |
| PATCH | `/{AppointmentId}/offering/{OfferingId}/change-price` | правка цены, Admin |
| PATCH | `/{AppointmentId}/offering/{OfferingId}/change-duration` | правка длительности, Admin |

### Settings — `/api/settings`

| Метод | Путь | Назначение |
|---|---|---|
| GET | `/` | настройки мастера, Admin |
| PUT | `/` | изменить настройки, Admin |
| POST | `/apply` | применить настройки к будущим дням, Admin |
| GET | `/contacts` | адрес и телефон салона, публично |

### Analytics — `/api/analytics`

Восемь endpoint'ов, доступ Admin: `gain`, `offerings-popularity`, `offerings-gain`, `no-show-rate`, `average-check`, `average-hour-gain`, `workload-rate`, `best-clients`. У `best-clients` обязателен параметр `limit`.

### Ошибки

`ExceptionHandlingMiddleware` возвращает `application/problem+json`:

| Ошибка | Код |
|---|---|
| валидация (`Results.ValidationProblem`) | 400 |
| `BusinessException` | 400 |
| `NotFoundException` | 404 |
| дубликат (`Results.Conflict`) | 409 |
| остальное | 500 без деталей |

`BusinessException → 400` — единый контракт. Решение зафиксировано в [TZ.md](./TZ.md) §5.

## Домен и правила

- `User`: `Phone` уникален, `Role: Default | Admin`. Тестовые доступы: Admin `+79991234567`, Client `+79341234567`.
- `Offering`: `Price: Money`, `Title` ≤ 50 символов, `Duration` > 10 минут, `IsActive`, `Category`.
- `Schedule`: `Date` уникальна, `WorkInterval` ≥ 2 часов, `BreakInterval` строго внутри рабочего времени и не граничит с ним.
- `Appointment`: `Interval`, `Status`, список услуг. Цена и длительность фиксируются в `AppointmentOffering` на момент записи.
- Запись отклоняется: в прошлом, вне рабочего времени, в перерыв, в выходной день, с пересечением активных записей.
- Отмена из `Confirmed` — не позже чем за 2 часа до начала. `Pending` отменяется без ограничений.
- Закрыть день можно только без активных записей.

## Проверки

```powershell
dotnet build backend/backend.sln
dotnet test backend/backend.sln
npm --prefix frontend/salon run lint
npm --prefix frontend/salon run build
```

`DomainTests` покрывают сущности (`Offering`, `Schedule`, `Appointment`, `User`, `VerificationCode`, `TelegramBinding`), Value Objects (`Money`, `TimeInterval`), `TimelineBuilder` и Telegram-уведомления.

CI (`.github/workflows/ci.yml`) запускает backend build/test и frontend lint/build на push и pull request в `main`.

## Конфигурация

| Где | Ключ | Назначение |
|---|---|---|
| `appsettings.Development.json` | `ConnectionStrings:SalonDbContext` | строка подключения PostgreSQL |
| `appsettings.Development.json` | `Jwt:Issuer`, `Jwt:Audience`, `Jwt:Key`, `Jwt:ExpiryMinutes` | параметры токена; срок — 30 дней |
| `appsettings.Development.json` | `Telegram:Bot:Token`, `Telegram:Bot:Username` | бот для кодов входа; не обязателен в dev |
| `appsettings.json` | `Cors:AllowedOrigins` | origins фронтенда, по умолчанию `localhost:3000` и `localhost:5173` |
| `.env` (Docker) | `POSTGRES_PASSWORD`, `JWT_KEY`, `TELEGRAM_BOT_TOKEN`, `PUBLIC_URL`, `DOMAIN` | секреты и адреса |

Секреты не коммитьте. Файлы `appsettings.Development.json` и `.env` в `.gitignore`. Правила — в [docs/security.md](./docs/security.md).

## Где получить помощь

- [docs/README.md](./docs/README.md) — карта документации.
- [docs/api.md](./docs/api.md) — форматы, статусы, коды ошибок.
- [docs/telegram-bot.md](./docs/telegram-bot.md) — настройка Telegram-бота.
- [TZ.md](./TZ.md) — требования и известные проблемы.
- [GitHub Issues](https://github.com/v2041/salon/issues) — вопросы и баги.

## Участие в разработке

1. Прочитайте [docs/workflow.md](./docs/workflow.md).
2. Добавьте правило в `Domain`. Сразу добавьте тест в `DomainTests`.
3. Запустите проверки: `dotnet build backend/backend.sln`, `dotnet test backend/backend.sln`, `npm --prefix frontend/salon run lint`, `npm --prefix frontend/salon run build`.
4. Обновите `README.md` и `TZ.md`.

Конвенции фронтенда — в [docs/frontend/conventions.md](./docs/frontend/conventions.md). Стиль текста — [STE100.md](./STE100.md). Ревью и коммиты — [docs/workflow.md](./docs/workflow.md).

## Лицензия и автор

Проект распространяется по лицензии MIT. Полный текст — в файле [LICENSE](./LICENSE).

Автор: [v2041](https://github.com/v2041) · makstara55@gmail.com
