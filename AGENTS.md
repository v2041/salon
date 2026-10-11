# AGENTS.md — Frontend salon (frontend/salon)

Next.js 16 (App Router), React 19, TypeScript strict, MUI 9, mobile-first.

## Структура

- `app/layout.tsx` — общий: `AppRouterCacheProvider` → `Box` → `BottomNavigationBar`.
- `app/(client)/` — route group без layout: `offerings/page.tsx`, `booking/page.tsx`, `my-appointments/page.tsx`.
- `components/` — `MyAppointmentsList.tsx`, `BottomNavigationBar.tsx`;
  `components/booking/` — `OfferingList`, `Calendar`, `TimeGrid`, `BookingSummaryBar`.
- `lib/format.ts` — `formatPrice`, `formatTime`, `formatDuration`, `toIsoDate`, `plural`, `timeToMinutes`.

## Компоненты

```tsx
"use client";   // только если есть state/handlers

type Props = { slots: string[]; selected: string | null; onSelect: (t: string) => void };

export default function TimeGrid({ slots, selected, onSelect }: Props) {
  return (
    <Box sx={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: 1 }}>
      {slots.map((t) => (
        <Button key={t} variant={selected === t ? "contained" : "outlined"} onClick={() => onSelect(t)}>{t}</Button>
      ))}
    </Box>
  );
}
```

Правила:

- `export default function`, пропсы — локальный `type Props`.
- `'use client'` — только при реальной необходимости (state, handlers, `useRouter`).
- MUI — именованные импорты из `@mui/material`, иконки из `@mui/icons-material` (алиас `...Icon`).
- Стилизация — только `sx`-проп с theme-токенами. Без `styled()`, `makeStyles`, инлайн-CSS.
- Импорты через alias: `@/components/...`, `@/lib/format`.
- Весь UI-текст на русском. Даты/время/цены — через `lib/format.ts` (ru-RU), не `toLocaleString` в местах.
- Mobile-first: нижняя навигация, `Box sx={{ pb: 12 }}` под контент, фиксированные бары — `position: fixed`.

## Данные и API

- Сейчас моки: `OFFERINGS`, `FREE_INTERVALS`, `AVAILABLE_DATES` в `booking/page.tsx`;
  `APPOINTMENTS` в `components/MyAppointmentsList.tsx`.
- Подключение API: `lib/api.ts`, `fetch(url, { credentials: "include" })` → `http://localhost:5164`.
  Форматы бекенда: даты `yyyy-MM-dd`, время `HH:mm`, деньги `{ value, currency }`.
- При уборке моков — не сломать flow: услуги → дата → время → подтверждение; смена услуг сбрасывает дату/время.

## Проверка

`npm run lint` и `npm run build` (type-check) после правок. Не трогать `node_modules/`, `.next/`.
