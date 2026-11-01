# Техническое задание: система управления салоном красоты (онлайн-запись, аналитика, витрина услуг)

Версия 1.2 · Сформировано на основании текущего кода (`backend/`, `frontend/salon`)
Обновлено 01.11.2026: спека #1 реализована, ревью 11.10.2026 закрыто (§2.4).

## 1. Общие сведения

**Цель.** Веб-система для самозанятого мастера (1 человек): онлайн-запись клиентов по номеру телефона, управление услугами и расписанием, аналитика дохода.

**Пользователи:**
| Роль | Кто | Что делает |
|---|---|---|
| Клиент | Любой посетитель сайта | Смотрит витрину услуг, записывается, отслеживает/отменяет свои записи |
| Мастер (Admin) | Владелец, 1 человек | Управляет услугами, расписанием, записями; смотрит аналитику |

**Стек (зафиксирован кодом):**
- Backend: .NET 10, Minimal API, PostgreSQL, EF Core, FluentValidation, DDD-архитектура (Domain / Features / Infrastructure / WebApi), JWT (Bearer через HttpOnly-cookie), xUnit.
- Frontend: Next.js 16 (App Router), React 19, TypeScript, MUI 9, mobile-first.

## 2. Текущее состояние (что уже реализовано)

### 2.1 Backend — есть
- **Доменная модель:** `User`, `Offering`, `Schedule`, `Appointment`, `AppointmentOffering` (снимок цены/длительности на момент записи); Value Objects `Money`, `TimeInterval`; enum'ы `AppointmentStatus` (Pending → Confirmed → Completed / Canceled / Missed / Rejected), `UserRole`.
- **Бизнес-правила в домене:** запрет записи в прошлом, внутри перерыва/не рабочего времени, с перекрытием; отмена не ранее чем за 2 часа до начала; рабочий день ≥ 2 ч; перерыв строго внутри рабочего времени и не граничит с ним.
- **API (сверено с кодом 11.10.2026, статус авторизации — фактический):**

| Метод | Путь | Назначение | Авторизация сейчас |
|---|---|---|---|
| POST | `/api/auth/send-code` | Выдать код подтверждения по телефону | нет (публичный) |
| POST | `/api/auth/register` | Регистрация → JWT-cookie | нет (публичный) |
| POST | `/api/auth/login` | Вход по телефону + код → JWT-cookie | нет (публичный) |
| POST | `/api/auth/logout` | Удалить cookie сессии | нет |
| GET | `/api/auth/me` | Текущий пользователь | **User ✓** |
| GET | `/api/offerings/` | Все активные услуги | нет (публичный) |
| GET | `/api/offerings/{id}` | Активная услуга; неактивная или неизвестная → 404 | нет (публичный) |
| GET | `/api/offerings/all` | Все услуги, включая неактивные | **Admin ✓** |
| POST | `/api/offerings/` | Создать услугу | **Admin ✓** |
| PUT | `/api/offerings/{id}` | Изменить услугу (feature зарегистрирована) | **Admin ✓** |
| DELETE | `/api/offerings/{id}` | Деактивировать услугу | **Admin ✓** (роль проверяется) |
| PATCH | `/api/offerings/{id}/restore` | Восстановить услугу | **Admin ✓** |
| GET | `/api/schedules/free-time?date=` | Свободные окна дня | нет (публичный) |
| GET | `/api/schedules/available-dates?offeringIds=` | Даты с доступным временем | нет (публичный) |
| GET | `/api/schedules/` | Будущие расписания | **Admin ✓** (строже ТЗ) |
| POST | `/api/schedules/` | Создать день | **Admin ✓** |
| PUT | `/api/schedules/` | Изменить день | **Admin ✓** |
| GET | `/api/appointments/{id}` | Запись | **Admin ✓** (строже ТЗ) |
| GET | `/api/appointments/all` | Все записи | **Admin ✓** |
| GET | `/api/appointments/user` | Записи текущего пользователя | **User ✓** |
| POST | `/api/appointments/` | Создать запись; `userId` **из JWT-claims ✓** | **User ✓** |
| PATCH | `/api/appointments/{id}/cancel` | Отмена своей записи (с проверкой владельца) | **User + владелец ✓** |
| PATCH | `/api/appointments/{id}/change-status` | Смена статуса (мастер) | **Admin ✓** |
| PATCH | `/api/appointments/{id}/change-time` | Перенос времени | **Admin ✓** |
| PATCH | `/api/appointments/{AppointmentId}/offering/{OfferingId}/change-price` | Правка цены в записи | **Admin ✓** |
| PATCH | `/api/appointments/{AppointmentId}/offering/{OfferingId}/change-duration` | Правка длительности услуги в записи | **Admin ✓** |
| GET | `/api/analytics/*` (11 шт.: gain, gain-by-month, offerings-popularity, offerings-gain, no-show-rate, average-check, average-hour-gain, workload-rate, best-clients, clients, losses) | Аналитика (вместо `schedules/gain`) | **Admin ✓** |
| GET | `/healthz` | Health check (БД) | нет (публичный), `200`/`503` |

`GET /api/schedules/gain` **удалён** — перенесён в `GET /api/analytics/gain`.
Пути `change-status` / `change-time` / `change-price` отличаются от §5 (§5 приведено к коду).

- **Фоновый сервис `ScheduleService`:** автогенерация расписания на 14 дней вперёд (ежедневно 10:00–18:00, перерыв 15:00–15:20, воскресенье — выходной), повтор каждые ~100 с.
- **Тесты:** `DomainTests` (xUnit) и `WebApiTests` (WebApplicationFactory + Testcontainers). Для HTTP-тестов нужен Docker.

### 2.2 Frontend — есть
- Каркас Next.js: `app/page.tsx`, `(app)/booking`, `(app)/my-appointments`, `(app)/admin`, `(app)/offerings/[id]`.
- Компоненты: `LoginScreen`, `booking/*` (`ServicePicker`, `Calendar`, `TimeGrid`), `MyAppointmentsList`, `admin/*` (услуги, расписание, записи, создание записи, аналитика), `ThemeRegistry`; утилиты `lib/format.ts`, `lib/useBookingFlow.ts`.
- Живой фронт: `lib/api.ts` (`offeringsApi`, `schedulesApi`, `appointmentsApi`, `authApi`, `analyticsApi`), `fetch(..., { credentials: "include" })`. Мок-данных нет.
- Экраны входа (FR-A*) есть. Кнопка отмены записи (FR-B5) и UI переноса времени (FR-M3) есть.

### 2.3 Функциональные куски — подключены, но с расхождениями
- `ChangeAppointmentTime` → зарегистрирован как `PATCH /api/appointments/{id}/change-time` (Admin).
- `ChangeAppointmentOfferingPrice` → `PATCH /api/appointments/{AppointmentId}/offering/{OfferingId}/change-price` (Admin, путь отличается от §5).
- `ChangeOffering` → `PUT /api/offerings/{id}` (Admin).

### 2.4 Известные проблемы (ревью 11.10.2026)

**Статус ревью 11.10.2026:** все пункты закрыты. Спека #1 реализована.

**Критично:**
1. Роль Admin — ✅ `User.PromoteToAdmin()`, сид и `AdminPhone` дают роль.
2. ✅ `PATCH {id}/change-time` требует роль Admin.
3. CORS — ✅ `AllowCredentials()` для origin'ов фронта.
4. Dev-код в `send-code` — ✅ в `Development` код приходит в ответе.

**Важно:**
5. FR-B6 гонка слотов — ✅ транзакция и уникальный индекс; конфликт → 400.
6. Неактивные услуги — ✅ исключены в `CreateAppointment` и `available-dates`.
7. FR-B5 — ✅ правило 2 часов для `Pending` и `Confirmed`; кнопка отмены на фронте.
8. FR-A4 — ✅ 5 кодов в час и 20 попыток в час → 429; `/healthz` реализован.
9. ✅ `DomainTests` и `WebApiTests`; покрыты отмена «2 часа», `AppointmentTimeChanger`, метрики и HTTP-контракт.
10. ✅ Login/Register дают `problem+json`; `change-status` на `Pending` даёт 400.
11. ✅ Сиды стабильны, конфликт EF Core и NU1903 закрыты.
12. ✅ Фронт: `Calendar` устойчив к пустому списку, навигация через `next/link`, мок-данных нет.

**Мелочи:** ✅ имена файлов аналитики исправлены, `toLocaleDateString` живёт только в `lib/format.ts`.

## 3. Функциональные требования

### 3.1 Аутентификация клиента (по телефону)
| ID | Требование | Приоритет |
|---|---|---|
| FR-A1 | Клиент вводит телефон → система отправляет SMS с одноразовым кодом (TTL ≤ 10 мин, один активный код на номер, повторная отправка заменяет старый) | P0 |
| FR-A2 | По неверному/истёкшему коду — ошибка; после входа/регистрации ставится HttpOnly JWT-cookie (30 дней) | P0 |
| FR-A3 | Повторный вход существующего номера = логин (без дубля); новый номер = регистрация по имени/фамилии | P0 |
| FR-A4 | Rate limiting: ≤ 5 кодов на номер в час, ≤ 20 попыток ввода кода в час | P1 |

> Реализация отправки SMS: выбрать провайдера (SMS.ru / Telegram Bot API как fallback). До подключения код не отправляется; для dev-проверок он читается из таблицы `VerificationCodes`.

### 3.2 Витрина услуг (клиент)
| ID | Требование | Приоритет |
|---|---|---|
| FR-V1 | Публичный список активных услуг: название, описание, цена, длительность; сгруппировано по категориям | P0 |
| FR-V2 | Категория услуги (enum/справочник: Волосы, Ногти, Лицо и т.д.) — новое поле `Offering.Category` + миграция | P1 |
| FR-V3 | Страница услуги с деталями и кнопкой «Записаться» (переход в flow с предвыбранной услугой) | P2 |

### 3.3 Онлайн-запись (клиент)
| ID | Требование | Приоритет |
|---|---|---|
| FR-B1 | Flow: выбор услуг (≥1, можно несколько) → дата → время → подтверждение; итоговая цена и суммарная длительность считаются на клиенте из данных API | P0 |
| FR-B2 | Даты — только из `available-dates` для выбранных услуг; слоты времени — из `free-time` с шагом 30 мин, отфильтрованные по суммарной длительности | P0 |
| FR-B3 | Запись создаётся только для авторизованного клиента (`userId` из JWT, не из тела запроса) | P0 |
| FR-B4 | Экран успеха с карточкой записи; запись видна в «Мои записи» со статусом Pending | P0 |
| FR-B5 | Отмена клиентом: только Pending/Confirmed и не позднее чем за 2 часа до начала (правило уже в домене) | P0 |
| FR-B6 | Защита от гонок: повторная попытка создания на занятый слот → понятная ошибка «время уже занято, выберите другое» | P1 |

### 3.4 Панель мастера
| ID | Требование | Приоритет |
|---|---|---|
| FR-M1 | **Услуги:** CRUD + деактивация/восстановление (feature `ChangeOffering` зарегистрировать как `PUT /api/offerings/{id}`), изменение цены/длительности/названия/описания | P0 |
| FR-M2 | **Расписание:** список дней с записями; создание/изменение дня (рабочее время, перерыв, выходной); отмена рабочего дня только без активных записей (правило в домене) | P0 |
| FR-M3 | **Записи:** список на день/неделю с фильтром по статусу; действия: подтвердить, отклонить, завершить, пропуск, отменить; перенос времени (`ChangeAppointmentTime` — зарегистрировать endpoint); изменение цены услуги в записи (`ChangeAppointmentOfferingPrice` — зарегистрировать) | P0 |
| FR-M4 | **Аналитика** (экран + API): см. 3.5 | P1 |

### 3.5 Аналитика
В `Features/Analytics` 11 endpoint'ов (Admin). FR-N1, FR-N2 и FR-N4 закрыты:
gain, gain-by-month, offerings-gain, offerings-popularity, no-show-rate, average-check,
average-hour-gain, workload-rate, best-clients, clients, losses.
| ID | Метрика | Приоритет |
|---|---|---|
| FR-N1 | Доход за период (есть) + разбивка по месяцам и по услугам | P1 |
| FR-N2 | Количество клиентов: всего / новых / повторных за период | P1 |
| FR-N3 | Топ услуг по выручке и по количеству записей | P1 |
| FR-N4 | Отказы/отмены/пропуски: количество и % (no-show rate) | P2 |
| FR-N5 | Средний чек, средний чек на клиента | P2 |
| API | Новый модуль `Features/Analytics`: `GET /api/analytics/summary?from=&to=`, `GET /api/analytics/by-offering?from=&to=` (доступ: Admin) | P1 |

### 3.6 Авторизация и роли (кросс-требование)
| ID | Требование | Приоритет |
|---|---|---|
| FR-S1 | Все управляющие endpoint'ы — `RequireAuthorization` + проверка роли Admin; клиентские — роль User; публичные: витрина, free-time, available-dates, auth | P0 |
| FR-S2 | `userId`/`phone` берутся из JWT-claims; передавать их в теле запросов запрещено (сейчас `CreateAppointmentRequest.UserId` — закрыть) | P0 |
| FR-S3 | Создание аккаунта мастера: первый зарегистрированный пользователь получает роль Admin, либо номер мастера задаётся в конфигурации (`AdminPhone`) | P0 |

## 4. Модель данных

### 4.1 Существующая (из кода)
```
User            Id, FirstName, LastName?, Phone(уникальный), CreatedAt, LastLoginAt, Role
Offering        Id, Price(Money), Title(≤50), Description, Duration(>10min), IsActive
Schedule        Id, Date(уникальная), IsWorking, WorkInterval(TimeInterval), BreakInterval?(⊂ Work)
Appointment     Id, ScheduleId, UserId, Interval, Status, Offerings[], CreatedAt, CompletedAt?
AppointmentOffering  OfferingId, Price(снимок), Duration(снимок)
VerificationCode Phone(ключ), Code, ExpiresAt, CreatedAt, AttemptsCount,
                 SendsInWindow, SendWindowStart, AttemptsInWindow, AttemptsWindowStart
```

Демо-стенд: `docker compose --profile demo run --rm --build seed` создает 60 клиентов, 22 услуги (19 активных), 142 дня и около 370 записей. Сид удаляет бизнес-данные; тестовые Admin и Client остаются. Порядок запуска — в README, раздел «Демо-данные для проверки».

### 4.2 Изменения
1. ✅ `Offering.Category` (enum) + индекс; миграция EF Core.
2. ✅ Уникальный индекс по `Phone` на `Users`.
3. ✅ `Appointment.CreatedAt`, `CompletedAt?` — для аналитики и аудита.
4. ✅ Снимок цены и длительности в `AppointmentOffering` — только через доменные методы. Правка длительности пересчитывает `Appointment.Interval` и проверяет работу, перерыв и другие записи.
5. ✅ Уникальный индекс по слоту `(ScheduleId, StartTime)` для активных статусов.
6. ✅ Счётчики лимитов `send-code` и попыток входа на `VerificationCode`.

## 5. API — изменения к текущему (статус 11.10.2026)

**Сделано:**
- ✅ `PUT /api/offerings/{id}` — зарегистрирован (Admin).
- ✅ Перенос времени — `PATCH /api/appointments/{id}/change-time` (Admin).
- ✅ Правка цены — `PATCH /api/appointments/{AppointmentId}/offering/{OfferingId}/change-price` (Admin).
- ✅ Правка длительности — `PATCH /api/appointments/{AppointmentId}/offering/{OfferingId}/change-duration` (Admin, 17.10.2026; 18.10.2026 — пересчёт интервала и проверки: работа, перерыв, другие записи).
- ✅ 18.10.2026: новый телефон в админ-записи → 404 «Клиент не зарегистрирован», мастер вводит имя и фамилию — карточка создаётся вместе с записью; 409 дней и услуг с текстом; `isWorking` в списке расписаний.
- ✅ 18.10.2026: «сейчас» для слотов и проверки прошлого — местное время (`DateTime.Now`), а не UTC: запись на уже прошедшее время того же дня отклоняется.
- ✅ `POST /api/appointments/`: `userId` берётся из JWT-claims, из тела убран.
- ✅ Авторизация по таблице 2.1 — проставлена.
- ✅ Отмена — отдельный `PATCH {id}/cancel` (User + проверка владельца); остальные переходы — `change-status` (Admin).
- ✅ Аналитика: 11 endpoint'ов вместо пары summary/by-offering (см. §2.1).
- ✅ 01.11.2026: `GET /healthz` с проверкой БД.
- ✅ 01.11.2026: `GET /api/auth/me`, `POST /api/auth/logout`.
- ✅ 01.11.2026: `GET /api/offerings/all` и `PATCH /api/offerings/{id}/restore`.
- ✅ 01.11.2026: rate limiting `send-code` (5 кодов/час, 20 попыток/час → 429), dev-код в ответе, `AdminPhone`.
- ✅ 01.11.2026: FR-N1/N2/N4 — `gain-by-month`, `clients`, `losses`.

- ✅ 19.10.2026: `Schedule.AddAppointment` и `AttachAppointment` отклоняют запись в выходной день. Правка длительности и перенос в тот же день в выходной день проходят (там только исторические записи).
- ✅ 19.10.2026: `TimelineBuilder` не отдаёт слоты до начала смены. Первый слот на сегодня — следующая минута.
- ✅ 19.10.2026: 500-ответы не раскрывают детали исключения. Код подтверждения хранится в UTC. Повторная отправка — не чаще 1 раза в минуту. Срок JWT приведён к сроку cookie — 30 дней (`ExpiryMinutes`).
- ✅ 19.10.2026: `offerings-gain` считает выручку по цене из записей (`AppointmentOffering.Price`), а не по текущему каталогу.

**Осталось:**
- Реальный SMS-провайдер и отправка SMS (вне текущего скопа).
- CI/CD-деплой, бэкапы БД и нагрузочное тестирование p95.

**Форматы:** JSON, даты ISO (`yyyy-MM-dd`, `HH:mm`; в ответах время — `HH:mm:ss`), деньги `{ value, currency }`.
Ошибки — `ExceptionHandlingMiddleware`: **фактически** BusinessException → 400, NotFound → 404,
валидация → ValidationProblem; остальное → 500 (расхождение: код даёт 400 там, где §5 предлагал 409/422 —
решение: оставить 400 как единый контракт, зафиксировано в README §4).

## 6. Ненормативные требования
| Категория | Требование |
|---|---|
| Производительность | p95 < 300 мс для витрины/free-time/available-dates при ≤ 10k записей; free-time считать по доменному `TimelineBuilder` (уже есть) |
| Безопасность | JWT в HttpOnly-cookie (есть); rate limiting на auth и создание записей; валидация телефона (+7, 10 цифр; send-code проверяется, 19.10.2026); секрет JWT из конфигурации/env; CORS из `Cors:AllowedOrigins` (параметризован 19.10.2026) |
| Наблюдаемость | Структурный логгер, health check, корреляция ошибок через middleware (есть) |
| Тесты | Домен: покрытие текущих правил + новые (категория, перенос, смена цены); интеграционные: happy path записи и конфликты слотов |
| UI | Mobile-first (MUI), RU-локализация дат/валюты, состояния загрузки/ошибки/пустых списков на всех экранах |
| Деплой | VPS + Docker Compose: Caddy (HTTPS, reverse proxy, порты 80/443) → frontend/API; миграции применяет сервис `migrate` перед стартом API; секреты в `.env`; мин. требования 1 vCPU / 2 GB RAM (из README) |

## 7. Этапы работ

**Этап 0 — «Критичные баги ревью» (P0)** — ✅ закрыт.
1. ✅ Роль Admin в сидах и `AdminPhone`.
2. ✅ `change-time` требует роль Admin.
3. ✅ CORS `AllowCredentials()` и dev-код в `send-code`.

**Этап 1 — «Клиентская часть живая» (P0)**
4. Реальный SMS-провайдер — вне текущего скопа.
5. ✅ Фронтенд подключён к API; мок-данных нет.
6. ✅ Авторизация endpoint'ов, роли, `userId` из токена (FR-S1..S3).

**Этап 2 — «Панель мастера» (P0)** — ✅ закрыт.
7. ✅ UI «Услуги» — создание, изменение, деактивация и восстановление (FR-M1).
8. ✅ UI расписания: дни, перерывы, выходные и записи.
9. ✅ UI записей: статусы, цена, длительность, перенос времени и фильтры.

**Этап 3 — «Аналитика» (P1)** — ✅ закрыт.
10. ✅ Экран мастера с метриками и графиками; FR-N1, FR-N2 и FR-N4 закрыты.

**Этап 4 — «Продакшн» (P1–P2)**
11. ✅ Rate limiting и `/healthz`. Осталось: CI/CD-деплой и бэкапы БД.
12. ✅ docker-compose для прода (Caddy + HTTPS, сервис `migrate`).

## 8. Открытые вопросы (решить до старта этапа 1)
1. Канал доставки кода: SMS-провайдер (какой?) или Telegram/VK? В README есть пункт «Интеграция с VK ID» — отменить в пользу phone+code?
2. Как создаётся аккаунт мастера (первый пользователь / номер из конфига)?
3. Категории услуг: фиксированный список или произвольный текст? Нужны ли фото/изображения услуг на витрине?
4. Оплата/депозит в системе — нет (только запись); подтверждаем, что платежи вне системы?
5. Напоминания клиенту перед записью (SMS/push) — в текущем скопе или отложить?
