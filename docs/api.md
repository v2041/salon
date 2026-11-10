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
- `send-code`, `login`, `register` и админ-запись принимают телефон в теле.
- JWT хранится в HttpOnly-cookie. Frontend отправляет `credentials: "include"`.

## Код входа

`POST /api/auth/send-code` принимает `{ "phone": "+7XXXXXXXXXX" }`.
Ответ: `expiresAt`, `isRegistered`, `status`, `bindingUrl`.

- `status: "sent"` — код отправлен в Telegram-бот, `bindingUrl` = `null`.
- `status: "binding_required"` — номер не привязан: откройте `bindingUrl` и нажмите «Поделиться номером» в боте.
- В Development без токена бота ответ содержит `code`.
- Повтор в течение минуты не отправляет второе сообщение.
- Сбой отправки → 500; код не сохраняется.

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
