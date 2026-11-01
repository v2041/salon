# Команды

## Backend

Рабочая папка — `backend/`.

```powershell
dotnet build
dotnet test
dotnet run --project WebApi
```

API: `http://localhost:5164`.

Миграции требуют запущенный PostgreSQL и `appsettings.Development.json`.

```powershell
dotnet ef database update --project Infrastructure --startup-project WebApi
dotnet ef migrations add <Name> --project Infrastructure --startup-project WebApi
```

## Frontend

Рабочая папка — `frontend/salon/`. Нужен Node >= 24.

```powershell
npm install
npm run lint
npm run build
npm run dev
```

Сайт: `http://localhost:3000`.

## Docker

Создайте `.env` из `.env.example`. Задайте `POSTGRES_PASSWORD` и `JWT_KEY`.

```powershell
docker compose up --build -d
docker compose logs -f api
docker compose down
```

Сайт открывается по `PUBLIC_URL` из `.env`. Frontend получает этот адрес как `NEXT_PUBLIC_API_URL`.

## Демо-данные

Сид удаляет бизнес-данные, клиентов и коды подтверждения. Тестовые Admin `+79991234567` и Client `+79341234567` остаются.

```powershell
docker compose stop api
docker compose --profile demo run --rm --build seed
docker compose start api
```

Остановите API перед сидом. Иначе `ScheduleService` изменит расписание.

Сервис `seed` задает `ALLOW_DEMO_SEED=true` и флаг `--reset`. Без `--reset` сид останавливается, если в БД уже есть данные. Код входа приходит в Telegram-бот после привязки номера. В dev-режиме без токена бота код пишется в лог API и приходит в ответе `send-code`.

## Проверка после правок

- Backend: `dotnet build` и `dotnet test`.
- Frontend: `npm run lint` и `npm run build`.
- Не фиксируйте число тестов в документации. Запускайте тесты.
