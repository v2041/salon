# Архитектура

Салон красоты для одного мастера: онлайн-запись, услуги, расписание и аналитика.
Детали продукта: `TZ.md`. API и быстрый старт: `README.md`.

## Backend

Backend использует DDD и vertical slices.

- `Domain/` — сущности, Value Objects, enum'ы, доменные сервисы, исключения.
  Бизнес-правила живут только здесь. Новая логика начинается с Domain и теста.
- `Features/` — по папке на сценарий. В папке лежат request/response DTO, валидатор, обработчик и регистрация роутера.
  Модули `<Сущность>Module.cs` подключают фичи.
- `Infrastructure/` — `SalonDbContext`, EF-конфигурации, миграции, JWT, фоновый `ScheduleService`.
- `WebApi/` — `Program.cs`, middleware, CORS, auth-политики, `appsettings`.
- `Seeder/` — демо-данные для Docker. Использует доменные методы, не прямой `Appointment.Create`.

Domain не ссылается на Infrastructure. HTTP-слой не содержит бизнес-правил.

## Frontend

Next.js 16 App Router, React 19, TypeScript strict, MUI 9. UI mobile-first.

Экраны лежат в `app/(app)/`: `booking`, `my-appointments`, `admin`.
Вход — `app/page.tsx`. Витрины `/offerings` нет.

Frontend не хранит бизнес-данные. Все экраны получают данные через `lib/api.ts`.

## Поток данных

1. Компонент вызывает обертку из `lib/api.ts`.
2. API проверяет запрос и auth.
3. Endpoint вызывает доменный метод.
4. EF Core сохраняет изменения в PostgreSQL.

Форматы и коды ответов: `api.md`.
Правила слоев: `backend/domain.md`, `backend/features.md`, `backend/infrastructure.md`, `backend/webapi.md`.
