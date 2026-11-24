# Frontend

Next.js 16 App Router, React 19, TypeScript strict, MUI 9. UI mobile-first. Node >= 24.

## Экраны

- `app/(app)/booking` — запись.
- `app/(app)/my-appointments` — записи клиента.
- `app/(app)/admin` — админка.
- `app/privacy` — политика обработки персональных данных.
- `app/page.tsx` — вход через VK ID.

Витрины `/offerings` нет. Не добавляйте статические мок-данные.

## Компоненты

- `export default function`.
- Пропсы — локальный `type Props` (в файле с несколькими компонентами — `type <Имя>Props`).
- `"use client"` — только для state, handlers или `useRouter`.
- MUI — именованные импорты из `@mui/material`. Иконки — из `@mui/icons-material`.
- Стили — только `sx` с theme-токенами. Без `styled()`, `makeStyles` и инлайн-CSS.
- Импорты — через alias `@/components/...`, `@/lib/...`.
- Весь UI-текст на русском.
- Даты, время и цены форматируйте через `lib/format.ts` (ru-RU).
- Mobile-first: нижняя навигация, `Box sx={{ pb: 12 }}` под контент, фиксированные бары.

## Данные и API

- Все экраны работают с API через `lib/api.ts`.
- Базовый URL — `NEXT_PUBLIC_API_URL`. Fallback — `http://localhost:5164`.
- В Docker `NEXT_PUBLIC_API_URL` равен `PUBLIC_URL`.
- Запросы идут с `credentials: "include"` из `apiFetch`.
- Виджет входа VK ID читает `NEXT_PUBLIC_VK_APP_ID` из `frontend/salon/.env.local` (файл в `.gitignore`). Перед входом клиент отмечает согласие на обработку персональных данных.
- Типы API — `lib/types.ts`.
- Форматы бекенда: дата `yyyy-MM-dd`, время `HH:mm:ss`, деньги `{ value, currency }`.
- Запись создается только с cookie роли `User`. Без cookie `create` вернет 401.
- Смена услуг сбрасывает дату и время.

Форматы и ошибки: `../api.md`.

## Проверка

```powershell
cd frontend/salon
npm run lint
npm run build
```

`npm run build` проверяет типы и сборку Next.js.
