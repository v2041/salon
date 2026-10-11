# Salon API

API для управления салоном красоты (запись клиентов, услуги, расписание).

## Технологии

- .NET 10
- PostgreSQL
- Entity Framework Core
- FluentValidation
- Minimal API
- Scalar / OpenAPI
- xUnit

## Архитектура

Проект построен на Domain-Driven Design (DDD):

Salon.API/
├── Domain/                    # Бизнес-логика
│   ├── Entities/              # Сущности
│   ├── ValueObjects/          # Value Objects
│   ├── Enums/                 # Перечисления
│   ├── Exceptions/            # Исключения
│   └── Services/              # Доменные сервисы
├── Infrastructure/            # Внешние зависимости
│   ├── Data/                  # DbContext
│   ├── Configurations/        # EF маппинг
│   └── BackgroundServices/    # Фоновые сервисы
├── Features/                  # Модули
│   ├── Appointments/          # Записи
│   ├── Offerings/             # Услуги
│   ├── Schedules/             # Расписание
│   └── Auth/                  # Аутентификация
└── WebApi/                    # HTTP слой
    ├── Middlewares/           # Обработка ошибок
    └── Program.cs             # Точка входа

Ключевые принципы: бизнес-логика внутри сущностей, Value Objects с проверками, Minimal API, feature-based структура.

## API Эндпоинты

### Записи (Appointments)

GET /api/appointments/{id} - Получить запись по ID (доступ: все)
GET /api/appointments/admin/all - Все записи (доступ: Admin)
GET /api/appointments/user - Записи пользователя (доступ: User)
POST /api/appointments/ - Создать запись (доступ: User)
PATCH /api/appointments/{id}/status - Изменить статус (доступ: Admin/User)

Создание записи:
POST /api/appointments/
{
  "date": "2026-08-10",
  "time": "14:30",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "offeringIds": ["7c9e6679-7425-40de-944b-e07fc1f90ae7"]
}

Ответ:
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "date": "2026-08-10",
  "interval": { "start": "14:30", "end": "16:00" },
  "status": 0,
  "price": { "value": 3500, "currency": "RUB" }
}

### Услуги (Offerings)

GET /api/offerings/{id} - Получить услугу (доступ: все)
GET /api/offerings/ - Все активные услуги (доступ: все)
POST /api/offerings/ - Создать услугу (доступ: Admin)
DELETE /api/offerings/{id} - Удалить услугу (доступ: Admin)

Создание услуги:
POST /api/offerings/
{
  "price": 1500,
  "title": "Стрижка женская",
  "description": "Стрижка с укладкой",
  "duration": "01:00:00"
}

### Расписание (Schedules)

GET /api/schedules/free-time?date=2026-08-10 - Свободные окна на день
GET /api/schedules/available-dates?offeringIds=guid1,guid2 - Даты с доступным временем
GET /api/schedules/ - Все расписания (будущие)
POST /api/schedules/ - Создать день (доступ: Admin)
PUT /api/schedules/ - Изменить день (доступ: Admin)

Свободное время:
GET /api/schedules/free-time?date=2026-08-10
Ответ: { "freeIntervals": [{ "start": "10:00", "end": "12:00" }] }

Доступные даты:
GET /api/schedules/available-dates?offeringIds=guid1,guid2
Ответ: { "dates": ["2026-08-10", "2026-08-12"] }

### Аутентификация (планируется)

POST /api/auth/request-code - Запросить код подтверждения
POST /api/auth/login - Вход по телефону + код

## Доменная модель

Appointment (Запись):
- Id, Schedule, UserId, Interval, Status, Price, Offerings
- Методы: Cancel(), Confirm(), Reject(), Complete(), Miss()
- Статусы: Pending (0), Confirmed (1), Completed (2), Canceled (3), Missed (4), Rejected (5)

Value Objects:
Money: var price = Money.FromDecimal(1500, "RUB"); // value >= 0, currency не пустой
TimeInterval: var interval = TimeInterval.Create(new TimeOnly(10,0), new TimeOnly(12,0)); // Методы: IsOverlapping(), IsInside()

## Тестирование

Покрытие:
- OfferingTests: создание, изменение цены/названия/длительности
- ScheduleTests: создание, добавление записи
- MoneyTests: валидация, форматирование
- TimeIntervalTests: пересечение, вхождение

Пример теста:
[Fact]
public void ChangeTitle_WithValidTitle_UpdatesTitle()
{
    var offering = Offering.Create(...);
    offering.ChangeTitle("Новое название");
    Assert.Equal("Новое название", offering.Title);
}

## Деплой

VPS (рекомендуется):
1. Установить .NET Runtime и PostgreSQL
2. Скопировать проект
3. Настроить appsettings.Production.json
4. Применить миграции: dotnet ef database update
5. Запустить через systemd: sudo systemctl start salon-api.service

Docker:
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Salon.API.dll"]

Минимальные требования: 1 ядро CPU, 1-2 GB RAM, 20 GB SSD, Ubuntu 22.04 / Debian 12

## TODO

[x] Базовая архитектура (DDD)
[x] CRUD для услуг, расписания, записей
[x] Валидация через FluentValidation
[x] Обработка ошибок через Middleware
[x] Тесты для доменной модели
[ ] JWT аутентификация
[ ] Интеграция с VK ID
[ ] Rate Limiting
[ ] Health Checks
[ ] Docker-compose для прода
[ ] CI/CD (GitHub Actions)

## Лицензия

MIT License. Свободно для использования и модификации.

## Контакты

Email: [makstara55@gmail.com]
GitHub: [github.com/v2041]
