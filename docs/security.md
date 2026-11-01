# Секреты и конфигурация

- `backend/WebApi/appsettings.Development.json` в `.gitignore`. В нем строка БД и `Jwt:Key`.
- `.env` в `.gitignore`. Шаблон — `.env.example`.
- Не коммитьте реальные `POSTGRES_PASSWORD`, `JWT_KEY` и `TELEGRAM_BOT_TOKEN`.
- `TELEGRAM_BOT_TOKEN` — токен бота от @BotFather. `TELEGRAM_BOT_USERNAME` — имя бота без `@`, не секрет.
- Не вставляйте секреты в код, логи, отчеты и сообщения коммитов.
- Не логируйте токены, cookie и строки подключения.
- Перед коммитом проверьте staged files. Секреты не должны попасть в индекс.

Тестовые Admin `+79991234567` и Client `+79341234567` — данные сида, не боевые аккаунты.
