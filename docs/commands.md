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

Для входа через VK ID создайте `frontend/salon/.env.local` с `NEXT_PUBLIC_VK_APP_ID` — id приложения VK ID. Файл в `.gitignore`.

## Docker

Создайте `.env` из `.env.example`. Задайте `POSTGRES_PASSWORD`, `JWT_KEY`, `VK_APP_ID`, `VK_SERVICE_TOKEN` и `ADMIN_PHONE`.

```powershell
docker compose up --build -d
docker compose logs -f api
docker compose down
```

Сайт открывается по `PUBLIC_URL` из `.env`. Frontend получает этот адрес как `NEXT_PUBLIC_API_URL`.

## Демо-данные

Сид удаляет бизнес-данные и клиентов. Тестовые Admin `+79991234567` и Client `+79341234567` остаются.

```powershell
docker compose stop api
docker compose --profile demo run --rm --build seed
docker compose start api
```

Остановите API перед сидом. Иначе `ScheduleService` изменит расписание.

Сервис `seed` задает `ALLOW_DEMO_SEED=true` и флаг `--reset`. Без `--reset` сид останавливается, если в БД уже есть данные. Роль Admin выдаётся при входе с телефона из `ADMIN_PHONE`.

## Проверка после правок

- Backend: `dotnet build` и `dotnet test`.
- Frontend: `npm run lint` и `npm run build`.
- Не фиксируйте число тестов в документации. Запускайте тесты.
