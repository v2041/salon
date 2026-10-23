# Документация для агентов

Корневой `AGENTS.md` — краткая точка входа. Этот файл — карта разделов.
Читайте только документ для текущей задачи.

| Задача | Документ |
|---|---|
| Команды, Docker, демо-сид | `commands.md` |
| Слои и поток данных | `architecture.md` |
| Форматы API, статусы, ошибки, auth | `api.md` |
| Бизнес-правила, сущности, Value Objects | `backend/domain.md` |
| Endpoint, DTO, валидаторы, модули | `backend/features.md` |
| EF Core, миграции, JWT, фоновые сервисы | `backend/infrastructure.md` |
| Program.cs, middleware, CORS | `backend/webapi.md` |
| xUnit-тесты домена | `backend/testing.md` |
| React, Next.js, MUI, API-клиент | `frontend/conventions.md` |
| Ревью, коммиты, README/TZ | `workflow.md` |
| Секреты и конфигурация | `security.md` |

## Правила документации

- Держите корневой `AGENTS.md` коротким. Правила слоя кладите в его документ.
- Не храните счетчики тестов. Запускайте `dotnet test`.
- Не перечисляйте все файлы и компоненты. Пути устаревают. Описывайте возможности.
- Обновляйте документ при смене стека, команд или конвенций.
- Примеры кода держите в профильном документе, не в корне.

## Структура

```text
docs/
├── README.md
├── commands.md
├── architecture.md
├── api.md
├── workflow.md
├── security.md
├── backend/
│   ├── domain.md
│   ├── features.md
│   ├── infrastructure.md
│   ├── webapi.md
│   └── testing.md
└── frontend/
    └── conventions.md
```
