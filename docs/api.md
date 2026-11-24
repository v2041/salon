# API

Контракт API — `docs/openapi.json`. Не выдумывай роуты. §4. Требования: `TZ.md` §2.1 и §5.
Машинный контракт: `docs/openapi.json`. Файл генерируется при сборке WebApi.

## Форматы

- Дата: `yyyy-MM-dd`.
- Время в запросе: `HH:mm`. В ответе: `HH:mm:ss`.
- Деньги: `{ "value": 3500, "currency": "RUB" }`.
- Ошибки: `application/problem+json`, сообщения на русском.

## Статусы и категории

Статусы записи: `0=Pending`, `1=Confirmed`, `2=Completed`, `3=Canceled`, `4=Missed`, `5=Rejected`.

Категории услуг: `0=Hair`, `1=Nails`, `2=Face`, `3=Other`.

Числа enum'ов закреплены в API. Не меняйте их.

## Авторизация

- `userId` берите из JWT-claims. Не принимайте `userId` из тела запроса (FR-S2).
- `phone` берите из claims, если маршрут работает с текущим пользователем.
- Админ-запись принимает телефон в теле.
- JWT хранится в HttpOnly-cookie. Frontend отправляет `credentials: "include"`.

## Вход через VK ID

`POST /api/auth/vk` принимает `{ code, deviceId, codeVerifier, redirectUrl, consent }`.
Ответ: `{ "isNew": true }` или `{ "isNew": false }`.

- `consent` обязателен. Без согласия → 400.
- `isNew: true` — клиент создан впервые; фронтенд показывает шаг «Проверьте имя».
- Без телефона из VK или с не российским номером → 400.
- Сбой VK ID → 400.
- Успех ставит HttpOnly JWT-cookie. Версия политики и дата согласия сохраняются.

`PATCH /api/auth/me` принимает `{ firstName, lastName? }` и возвращает 204.

## Ошибки и коды ответов

- Валидация: `Results.ValidationProblem` → 400.
- `BusinessException` → 400.
- `NotFoundException` → 404.
- Дубликат: `Results.Conflict` → 409.
- Успех: `Created` (201), `Ok` (200), `NoContent` (204).

Middleware обрабатывает исключения. Не логируйте одно исключение дважды.

## Аналитика

Эндпоинт `best-clients` требует параметр `limit`. Без него API возвращает 400.

## Пагинация списков записей

`GET /api/appointments/user` и `GET /api/appointments/all` возвращают страницу записей.

- `scope` — обязателен: `upcoming` или `past`. Граница — время начала: позже текущего момента → `upcoming`, иначе → `past`. Статус не влияет.
- `page` — номер страницы, с 1. По умолчанию 1.
- `pageSize` — размер страницы, от 1 до 100. По умолчанию 20.
- `status` — только у `/all`: число 0–5. Без параметра — все статусы.

Ответ: `{ "items": [...], "page": 1, "pageSize": 20, "totalCount": 57 }`. `totalCount` — число записей раздела с учётом `status`.

Порядок: `upcoming` — от ближайшей, `past` — от самой свежей. Неверные параметры → 400.

## Изменения контракта

Сверяйте изменения с `README.md` §4 и `TZ.md` §2.1, §5.
Новые требования берите из `TZ.md` §3. Упоминайте ID вида `FR-B1` в описании работы.
