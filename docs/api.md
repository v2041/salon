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

## Ошибки и коды ответов

- Валидация: `Results.ValidationProblem` → 400.
- `BusinessException` → 400.
- `NotFoundException` → 404.
- Дубликат: `Results.Conflict` → 409.
- Успех: `Created` (201), `Ok` (200), `NoContent` (204).

Middleware обрабатывает исключения. Не логируйте одно исключение дважды.

## Аналитика

Эндпоинт `best-clients` требует параметр `limit`. Без него API возвращает 400.

## Изменения контракта

Сверяйте изменения с `README.md` §4 и `TZ.md` §2.1, §5.
Новые требования берите из `TZ.md` §3. Упоминайте ID вида `FR-B1` в описании работы.
