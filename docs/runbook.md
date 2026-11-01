# Как поднять проект

Два пути: локальная разработка и Docker. Локальный путь быстрее для правок. Docker ближе к продакшену.

## 1. Что нужно

- .NET SDK 10 (`dotnet --version`).
- Node.js 24 и npm (`node --version`).
- PostgreSQL 15+ — для локального пути.
- Docker Desktop — для Docker-пути и для тестов `WebApiTests`.
- Инструмент EF Core: `dotnet tool install --global dotnet-ef`.

Проверьте инструменты:

```powershell
dotnet --version
node --version
dotnet ef --version
docker ps
```

## 2. Локальная разработка

### 2.1 База данных

Запустите PostgreSQL. Создайте базу:

```powershell
psql -U postgres -c "CREATE DATABASE salon;"
```

Создайте `backend/WebApi/appsettings.Development.json`. Файл в `.gitignore`.

```json
{
  "ConnectionStrings": {
    "SalonDbContext": "User ID=postgres;Password=<пароль>;Host=localhost;Port=5432;Database=salon;"
  },
  "Jwt": {
    "Issuer": "SalonApi",
    "Audience": "SalonApi",
    "Key": "<строка не короче 32 символов>",
    "ExpiryMinutes": 43200
  },
  "AdminPhone": "<ваш телефон +7XXXXXXXXXX>"
}
```

Поле `AdminPhone` даёт роль Admin при регистрации. Оставьте пустым, если Admin не нужен.

### 2.2 Backend

```powershell
cd backend
dotnet ef database update --project Infrastructure --startup-project WebApi
dotnet run --project WebApi
```

- API: http://localhost:5164
- Scalar UI: http://localhost:5164/scalar/v1 (только Development)

### 2.3 Frontend

Во втором терминале:

```powershell
cd frontend/salon
npm install
npm run dev
```

Сайт: http://localhost:3000. Экраны: `/booking`, `/my-appointments`, `/admin`.

Frontend обращается к `NEXT_PUBLIC_API_URL`. По умолчанию — `http://localhost:5164`. Значение по умолчанию подходит для локального пути.

### 2.4 Вход и Admin

1. Введите телефон на экране входа.
2. В Development ответ `send-code` содержит `code`. SMS нет.
3. Введите код. Новый номер создаёт аккаунт.
4. Чтобы получить Admin, укажите `AdminPhone` и зарегистрируйтесь с этого номера.

## 3. Docker

Стек: PostgreSQL 17, API, frontend, Caddy (HTTPS).

1. Создайте `.env` из `.env.example`.

```powershell
Copy-Item .env.example .env
```

2. Задайте в `.env`:
   - `POSTGRES_PASSWORD` — пароль БД;
   - `JWT_KEY` — случайная строка не короче 32 символов;
   - `PUBLIC_URL` — публичный адрес: `http://localhost` или `https://salon.example.com`;
   - `DOMAIN` — домен Caddy: `:80` для локальной проверки;
   - `SALON_ADDRESS`, `SALON_PHONE` — контакты на экране записи (необязательно).

3. Запустите стек.

```powershell
docker compose up --build -d
docker compose logs -f api
```

Сайт открывается по `PUBLIC_URL`.

4. Полезные команды.

| Команда | Действие |
|---|---|
| `docker compose ps` | Состояние сервисов |
| `docker compose logs -f api` | Логи API |
| `docker compose down` | Остановить стек |
| `docker compose exec db psql -U salon -d salon` | Консоль БД |

### Демо-данные

Сид создаёт 60 клиентов, 22 услуги и расписание. Он удаляет бизнес-данные.

```powershell
docker compose stop api
docker compose --profile demo run --rm --build seed
docker compose start api
```

Остановите API перед сидом. Иначе `ScheduleService` изменит расписание.

Тестовые аккаунты остаются: Admin `+79991234567`, Client `+79341234567`. Код входа лежит в таблице `VerificationCodes`.

## 4. Проверка после правок

```powershell
dotnet build backend/backend.sln
dotnet test backend/backend.sln
npm --prefix frontend/salon run lint
npm --prefix frontend/salon run build
```

`dotnet test` запускает `DomainTests` и `WebApiTests`. `WebApiTests` поднимает PostgreSQL через Testcontainers. Docker Desktop должен работать.

## 5. Частые проблемы

- **`dotnet ef` не найден.** Установите: `dotnet tool install --global dotnet-ef`.
- **API падает на старте.** Проверьте `appsettings.Development.json` и доступность PostgreSQL.
- **Frontend получает 401.** Войдите. Cookie `some-cookies` ставится при `login` или `register`.
- **CORS-ошибка.** Добавьте origin фронтенда в `Cors:AllowedOrigins`.
- **`dotnet test` падает на Testcontainers.** Запустите Docker Desktop. Проверьте `docker ps`.
- **Testcontainers: `Access to the path is denied` для `npipe`.** Включите в Docker Desktop настройку `Expose daemon on tcp://localhost:2375 without TLS`. Затем задайте `DOCKER_HOST`:

```powershell
$env:DOCKER_HOST = "tcp://localhost:2375"
dotnet test backend/backend.sln
```

- **Порт занят.** Backend слушает 5164. Frontend — 3000. Caddy — 80 и 443.
