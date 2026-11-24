# Секреты и конфигурация

- `backend/WebApi/appsettings.Development.json` в `.gitignore`. В нем строка БД, `Jwt:Key` и `VkId:ServiceToken`.
- `.env` в `.gitignore`. Шаблон — `.env.example`.
- Не коммитьте реальные `POSTGRES_PASSWORD`, `JWT_KEY` и `VK_SERVICE_TOKEN`.
- `VK_SERVICE_TOKEN` — сервисный ключ приложения VK ID из кабинета id.vk.ru. `VK_APP_ID` и `NEXT_PUBLIC_VK_APP_ID` — не секреты. `ADMIN_PHONE` — телефон мастера.
- Не вставляйте секреты в код, логи, отчеты и сообщения коммитов.
- Не логируйте токены, cookie и строки подключения.
- Перед коммитом проверьте staged files. Секреты не должны попасть в индекс.

Тестовые Admin `+79991234567` и Client `+79341234567` — данные сида, не боевые аккаунты.
