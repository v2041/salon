-- ============================================================
-- ТЕСТОВЫЕ ДАННЫЕ ДЛЯ САЛОНА (заполняет и прошлое, и будущее)
-- Запуск: psql -h localhost -U postgres -d salon -f seed-past-data.sql
-- Статусы: 0=Pending, 1=Confirmed, 2=Completed, 3=Canceled, 4=Missed, 5=Rejected
-- ============================================================

-- ---------- 1) КЛИЕНТЫ (без пересечения с сидовыми +79... телефоны) ----------
INSERT INTO "Users" ("Id", "FirstName", "LastName", "Phone", "CreatedAt", "LastLoginAt", "Role")
SELECT v.* FROM (VALUES
  ('aaaaaaa1-0000-0000-0000-000000000001'::uuid, 'Анна',   'Смирнова',  '+79210000001', now() - interval '90 days', now() - interval '1 day',  0),
  ('aaaaaaa1-0000-0000-0000-000000000002'::uuid, 'Мария',  'Кузнецова', '+79210000002', now() - interval '60 days', now() - interval '2 day',  0),
  ('aaaaaaa1-0000-0000-0000-000000000003'::uuid, 'Елена',  'Попова',    '+79210000003', now() - interval '45 days', now() - interval '3 day',  0),
  ('aaaaaaa1-0000-0000-0000-000000000004'::uuid, 'Ольга',  'Васильева', '+79210000004', now() - interval '30 days', now() - interval '5 day',  0),
  ('aaaaaaa1-0000-0000-0000-000000000005'::uuid, 'Ирина',  'Новикова',  '+79210000005', now() - interval '14 days', now() - interval '7 day',  0),
  ('aaaaaaa1-0000-0000-0000-000000000006'::uuid, 'Татьяна','Морозова',  '+79210000006', now() - interval '7 days',  now() - interval '10 day', 0)
) AS v("Id","FirstName","LastName","Phone","CreatedAt","LastLoginAt","Role")
WHERE NOT EXISTS (SELECT 1 FROM "Users" u WHERE u."Phone" = v."Phone");

-- ---------- 2) УСЛУГИ ----------
INSERT INTO "Offerings" ("Id", "Title", "Description", "Price", "Price_Currency", "Duration", "IsActive", "Category")
SELECT v.* FROM (VALUES
  ('bbbbbbb2-0000-0000-0000-000000000001'::uuid, 'Женская стрижка',   'Стрижка + укладка',              1800.00, 'RUB', interval '1 hour',    true, 0),
  ('bbbbbbb2-0000-0000-0000-000000000002'::uuid, 'Окрашивание корней','Ammonia-free краска',            4500.00, 'RUB', interval '2 hours',   true, 0),
  ('bbbbbbb2-0000-0000-0000-000000000003'::uuid, 'Маникюр классический','Покрытие гель-лаком включено', 2000.00, 'RUB', interval '1 hour',    true, 1),
  ('bbbbbbb2-0000-0000-0000-000000000004'::uuid, 'Педикюр',           'Аппаратный + покрытие',          2500.00, 'RUB', interval '90 mins',   true, 1),
  ('bbbbbbb2-0000-0000-0000-000000000005'::uuid, 'Чистка лица',       'Комбинированная, УЗ',            3200.00, 'RUB', interval '1 hour 30 mins', true, 2),
  ('bbbbbbb2-0000-0000-0000-000000000006'::uuid, 'Массаж лица',       'Лимфодренажный',                 2800.00, 'RUB', interval '45 mins',   true, 2),
  ('bbbbbbb2-0000-0000-0000-000000000007'::uuid, 'Ламинирование ресниц','С ботоксом',                   2600.00, 'RUB', interval '1 hour 30 mins', true, 3),
  ('bbbbbbb2-0000-0000-0000-000000000008'::uuid, 'Брови',             'Коррекция + окрашивание',        1200.00, 'RUB', interval '40 mins',   true, 3),
  ('bbbbbbb2-0000-0000-0000-000000000009'::uuid, 'Старая услуга',     'Деактивирована для примера',     900.00,  'RUB', interval '30 mins',   false, 3)
) AS v("Id","Title","Description","Price","Price_Currency","Duration","IsActive","Category")
WHERE NOT EXISTS (SELECT 1 FROM "Offerings" o WHERE o."Title" = v."Title");

-- ---------- 3) ПРОШЛЫЕ РАБОЧИЕ ДНИ (для аналитики) ----------
INSERT INTO "Schedules" ("Id", "Date", "IsWorking", "WorkStartTime", "WorkEndTime", "BreakStartTime", "BreakEndTime")
SELECT v.* FROM (VALUES
  ('ccccccc3-0000-0000-0000-000000000001'::uuid, CURRENT_DATE - 21, true, '10:00'::time, '18:00'::time, '15:00'::time, '15:20'::time),
  ('ccccccc3-0000-0000-0000-000000000002'::uuid, CURRENT_DATE - 20, true, '10:00'::time, '18:00'::time, '15:00'::time, '15:20'::time),
  ('ccccccc3-0000-0000-0000-000000000003'::uuid, CURRENT_DATE - 19, true, '10:00'::time, '18:00'::time, '15:00'::time, '15:20'::time),
  ('ccccccc3-0000-0000-0000-000000000004'::uuid, CURRENT_DATE - 16, true, '10:00'::time, '18:00'::time, '15:00'::time, '15:20'::time),
  ('ccccccc3-0000-0000-0000-000000000005'::uuid, CURRENT_DATE - 15, true, '10:00'::time, '18:00'::time, '15:00'::time, '15:20'::time),
  ('ccccccc3-0000-0000-0000-000000000006'::uuid, CURRENT_DATE - 14, true, '11:00'::time, '17:00'::time, NULL::time, NULL::time),
  ('ccccccc3-0000-0000-0000-000000000007'::uuid, CURRENT_DATE - 9,  true, '10:00'::time, '18:00'::time, '15:00'::time, '15:20'::time),
  ('ccccccc3-0000-0000-0000-000000000008'::uuid, CURRENT_DATE - 8,  true, '10:00'::time, '18:00'::time, '15:00'::time, '15:20'::time),
  ('ccccccc3-0000-0000-0000-000000000009'::uuid, CURRENT_DATE - 7,  true, '10:00'::time, '18:00'::time, '15:00'::time, '15:20'::time),
  ('ccccccc3-0000-0000-0000-00000000000a'::uuid, CURRENT_DATE - 4,  true, '12:00'::time, '19:00'::time, '15:30'::time, '15:50'::time),
  ('ccccccc3-0000-0000-0000-00000000000b'::uuid, CURRENT_DATE - 3,  true, '10:00'::time, '18:00'::time, '15:00'::time, '15:20'::time),
  ('ccccccc3-0000-0000-0000-00000000000c'::uuid, CURRENT_DATE - 2,  true, '10:00'::time, '18:00'::time, '15:00'::time, '15:20'::time),
  ('ccccccc3-0000-0000-0000-00000000000d'::uuid, CURRENT_DATE - 1,  true, '10:00'::time, '18:00'::time, '15:00'::time, '15:20'::time)
) AS v("Id","Date","IsWorking","WorkStartTime","WorkEndTime","BreakStartTime","BreakEndTime")
WHERE NOT EXISTS (SELECT 1 FROM "Schedules" s WHERE s."Date" = v."Date");

-- ---------- 4) ПРОШЛЫЕ ЗАПИСИ (разные статусы) ----------
-- Колонки снимка услуги: Price, Price_Currency, Duration
INSERT INTO "Appointments" ("Id", "ScheduleId", "UserId", "StartTime", "EndTime", "Status")
SELECT v."Id", s."Id" AS "ScheduleId", v."UserId", v."StartTime", v."EndTime", v."Status"
FROM (VALUES
  -- 21 день назад: Анна стрижка 10:00-11:00 Completed, Мария маникюр 11:30-12:30 Completed
  ('ddddddd4-0000-0000-0000-000000000001'::uuid, CURRENT_DATE - 21, 'aaaaaaa1-0000-0000-0000-000000000001'::uuid, '10:00'::time, '11:00'::time, 2),
  ('ddddddd4-0000-0000-0000-000000000002'::uuid, CURRENT_DATE - 21, 'aaaaaaa1-0000-0000-0000-000000000002'::uuid, '11:30'::time, '12:30'::time, 2),
  -- 20 дней назад: Елена окрашивание 12:00-14:00 Completed, Ольга не пришла 14:30-15:30 Missed
  ('ddddddd4-0000-0000-0000-000000000003'::uuid, CURRENT_DATE - 20, 'aaaaaaa1-0000-0000-0000-000000000003'::uuid, '12:00'::time, '14:00'::time, 2),
  ('ddddddd4-0000-0000-0000-000000000004'::uuid, CURRENT_DATE - 20, 'aaaaaaa1-0000-0000-0000-000000000004'::uuid, '14:30'::time, '15:30'::time, 4),
  -- 19 дней назад: Анна чистка лица 10:00-11:30 Completed, Ирина брови 13:00-13:40 Rejected
  ('ddddddd4-0000-0000-0000-000000000005'::uuid, CURRENT_DATE - 19, 'aaaaaaa1-0000-0000-0000-000000000001'::uuid, '10:00'::time, '11:30'::time, 2),
  ('ddddddd4-0000-0000-0000-000000000006'::uuid, CURRENT_DATE - 19, 'aaaaaaa1-0000-0000-0000-000000000005'::uuid, '13:00'::time, '13:40'::time, 5),
  -- 16 дней назад: Мария педикюр 10:00-11:30 Completed
  ('ddddddd4-0000-0000-0000-000000000007'::uuid, CURRENT_DATE - 16, 'aaaaaaa1-0000-0000-0000-000000000002'::uuid, '10:00'::time, '11:30'::time, 2),
  -- 15 дней назад: Елена массаж 11:00-11:45 Completed, Анна отменилась 13:00-14:00 Canceled
  ('ddddddd4-0000-0000-0000-000000000008'::uuid, CURRENT_DATE - 15, 'aaaaaaa1-0000-0000-0000-000000000003'::uuid, '11:00'::time, '11:45'::time, 2),
  ('ddddddd4-0000-0000-0000-000000000009'::uuid, CURRENT_DATE - 15, 'aaaaaaa1-0000-0000-0000-000000000001'::uuid, '13:00'::time, '14:00'::time, 3),
  -- 14 дней назад: Ирина ламинирование 11:30-13:00 Completed
  ('ddddddd4-0000-0000-0000-00000000000a'::uuid, CURRENT_DATE - 14, 'aaaaaaa1-0000-0000-0000-000000000005'::uuid, '11:30'::time, '13:00'::time, 2),
  -- 9 дней назад: Анна маникюр 10:00-11:00 Completed
  ('ddddddd4-0000-0000-0000-00000000000b'::uuid, CURRENT_DATE - 9,  'aaaaaaa1-0000-0000-0000-000000000001'::uuid, '10:00'::time, '11:00'::time, 2),
  -- 8 дней назад: Мария чистка 10:00-11:30 Completed, Ольга брови 12:00-12:40 Missed
  ('ddddddd4-0000-0000-0000-00000000000c'::uuid, CURRENT_DATE - 8,  'aaaaaaa1-0000-0000-0000-000000000002'::uuid, '10:00'::time, '11:30'::time, 2),
  ('ddddddd4-0000-0000-0000-00000000000d'::uuid, CURRENT_DATE - 8,  'aaaaaaa1-0000-0000-0000-000000000004'::uuid, '12:00'::time, '12:40'::time, 4),
  -- 7 дней назад: Елена педикюр 11:00-12:30 Completed
  ('ddddddd4-0000-0000-0000-00000000000e'::uuid, CURRENT_DATE - 7,  'aaaaaaa1-0000-0000-0000-000000000003'::uuid, '11:00'::time, '12:30'::time, 2),
  -- 4 дня назад (день 12:00-19:00): Анна окрашивание 12:30-14:30 Completed, Ирина стрижка 15:00-16:00 Completed
  ('ddddddd4-0000-0000-0000-00000000000f'::uuid, CURRENT_DATE - 4,  'aaaaaaa1-0000-0000-0000-000000000001'::uuid, '12:30'::time, '14:30'::time, 2),
  ('ddddddd4-0000-0000-0000-000000000010'::uuid, CURRENT_DATE - 4,  'aaaaaaa1-0000-0000-0000-000000000005'::uuid, '15:00'::time, '16:00'::time, 2),
  -- 3 дня назад: Мария массаж 10:00-10:45 Completed, Татьяна маникюр 11:00-12:00 Completed
  ('ddddddd4-0000-0000-0000-000000000011'::uuid, CURRENT_DATE - 3,  'aaaaaaa1-0000-0000-0000-000000000002'::uuid, '10:00'::time, '10:45'::time, 2),
  ('ddddddd4-0000-0000-0000-000000000012'::uuid, CURRENT_DATE - 3,  'aaaaaaa1-0000-0000-0000-000000000006'::uuid, '11:00'::time, '12:00'::time, 2),
  -- 2 дня назад: Анна брови 10:00-10:40 Completed, Татьяна чистка 13:00-14:30 Completed
  ('ddddddd4-0000-0000-0000-000000000013'::uuid, CURRENT_DATE - 2,  'aaaaaaa1-0000-0000-0000-000000000001'::uuid, '10:00'::time, '10:40'::time, 2),
  ('ddddddd4-0000-0000-0000-000000000014'::uuid, CURRENT_DATE - 2,  'aaaaaaa1-0000-0000-0000-000000000006'::uuid, '13:00'::time, '14:30'::time, 2),
  -- 1 день назад: Ольга педикюр 10:00-11:30 Completed, Ирина маникюр 12:00-13:00 Missed
  ('ddddddd4-0000-0000-0000-000000000015'::uuid, CURRENT_DATE - 1,  'aaaaaaa1-0000-0000-0000-000000000004'::uuid, '10:00'::time, '11:30'::time, 2),
  ('ddddddd4-0000-0000-0000-000000000016'::uuid, CURRENT_DATE - 1,  'aaaaaaa1-0000-0000-0000-000000000005'::uuid, '12:00'::time, '13:00'::time, 4)
) AS v("Id","Date","UserId","StartTime","EndTime","Status")
JOIN "Schedules" s ON s."Date" = v."Date"
WHERE NOT EXISTS (SELECT 1 FROM "Appointments" a WHERE a."Id" = v."Id");

-- Снимки услуг в записях (цена и длительность на момент записи)
INSERT INTO "AppointmentOffering" ("AppointmentId", "OfferingId", "Price", "Price_Currency", "Duration")
SELECT a."Id",
       o."Id",
       o."Price",
       o."Price_Currency",
       o."Duration"
FROM "Appointments" a
JOIN "Schedules" s  ON s."Id" = a."ScheduleId"
JOIN (VALUES
  (CURRENT_DATE - 21, '10:00'::time, 'bbbbbbb2-0000-0000-0000-000000000001'::uuid),
  (CURRENT_DATE - 21, '11:30'::time, 'bbbbbbb2-0000-0000-0000-000000000003'::uuid),
  (CURRENT_DATE - 20, '12:00'::time, 'bbbbbbb2-0000-0000-0000-000000000002'::uuid),
  (CURRENT_DATE - 20, '14:30'::time, 'bbbbbbb2-0000-0000-0000-000000000003'::uuid),
  (CURRENT_DATE - 19, '10:00'::time, 'bbbbbbb2-0000-0000-0000-000000000005'::uuid),
  (CURRENT_DATE - 19, '13:00'::time, 'bbbbbbb2-0000-0000-0000-000000000008'::uuid),
  (CURRENT_DATE - 16, '10:00'::time, 'bbbbbbb2-0000-0000-0000-000000000004'::uuid),
  (CURRENT_DATE - 15, '11:00'::time, 'bbbbbbb2-0000-0000-0000-000000000006'::uuid),
  (CURRENT_DATE - 15, '13:00'::time, 'bbbbbbb2-0000-0000-0000-000000000002'::uuid),
  (CURRENT_DATE - 14, '11:30'::time, 'bbbbbbb2-0000-0000-0000-000000000007'::uuid),
  (CURRENT_DATE - 9,  '10:00'::time, 'bbbbbbb2-0000-0000-0000-000000000003'::uuid),
  (CURRENT_DATE - 8,  '10:00'::time, 'bbbbbbb2-0000-0000-0000-000000000005'::uuid),
  (CURRENT_DATE - 8,  '12:00'::time, 'bbbbbbb2-0000-0000-0000-000000000008'::uuid),
  (CURRENT_DATE - 7,  '11:00'::time, 'bbbbbbb2-0000-0000-0000-000000000004'::uuid),
  (CURRENT_DATE - 4,  '12:30'::time, 'bbbbbbb2-0000-0000-0000-000000000002'::uuid),
  (CURRENT_DATE - 4,  '15:00'::time, 'bbbbbbb2-0000-0000-0000-000000000001'::uuid),
  (CURRENT_DATE - 3,  '10:00'::time, 'bbbbbbb2-0000-0000-0000-000000000006'::uuid),
  (CURRENT_DATE - 3,  '11:00'::time, 'bbbbbbb2-0000-0000-0000-000000000003'::uuid),
  (CURRENT_DATE - 2,  '10:00'::time, 'bbbbbbb2-0000-0000-0000-000000000008'::uuid),
  (CURRENT_DATE - 2,  '13:00'::time, 'bbbbbbb2-0000-0000-0000-000000000005'::uuid),
  (CURRENT_DATE - 1,  '10:00'::time, 'bbbbbbb2-0000-0000-0000-000000000004'::uuid),
  (CURRENT_DATE - 1,  '12:00'::time, 'bbbbbbb2-0000-0000-0000-000000000003'::uuid)
) AS av("Date","StartTime","OfferingId")
  ON av."Date" = s."Date" AND av."StartTime" = a."StartTime"
JOIN "Offerings" o  ON o."Id" = av."OfferingId"
WHERE NOT EXISTS (
  SELECT 1 FROM "AppointmentOffering" ao WHERE ao."AppointmentId" = a."Id"
);

-- ---------- 5) БУДУЩИЕ ДНИ И ЗАПИСИ ----------
-- Будущие рабочие дни: +1..+4, +6, +7 (10:00-18:00, перерыв 15:00-15:20)
INSERT INTO "Schedules" ("Id", "Date", "IsWorking", "WorkStartTime", "WorkEndTime", "BreakStartTime", "BreakEndTime")
SELECT v.* FROM (VALUES
  ('ccccccc3-0000-0000-0000-000000000101'::uuid, CURRENT_DATE + 1, true, '10:00'::time, '18:00'::time, '15:00'::time, '15:20'::time),
  ('ccccccc3-0000-0000-0000-000000000102'::uuid, CURRENT_DATE + 2, true, '10:00'::time, '18:00'::time, '15:00'::time, '15:20'::time),
  ('ccccccc3-0000-0000-0000-000000000103'::uuid, CURRENT_DATE + 3, true, '10:00'::time, '18:00'::time, '15:00'::time, '15:20'::time),
  ('ccccccc3-0000-0000-0000-000000000104'::uuid, CURRENT_DATE + 4, true, '10:00'::time, '18:00'::time, '15:00'::time, '15:20'::time),
  ('ccccccc3-0000-0000-0000-000000000105'::uuid, CURRENT_DATE + 6, true, '10:00'::time, '18:00'::time, '15:00'::time, '15:20'::time),
  ('ccccccc3-0000-0000-0000-000000000106'::uuid, CURRENT_DATE + 7, true, '10:00'::time, '18:00'::time, '15:00'::time, '15:20'::time)
) AS v("Id","Date","IsWorking","WorkStartTime","WorkEndTime","BreakStartTime","BreakEndTime")
WHERE NOT EXISTS (SELECT 1 FROM "Schedules" s WHERE s."Date" = v."Date");

-- Будущие записи: Pending (0) и Confirmed (1)
INSERT INTO "Appointments" ("Id", "ScheduleId", "UserId", "StartTime", "EndTime", "Status")
SELECT v."Id", s."Id", v."UserId", v."StartTime", v."EndTime", v."Status"
FROM (VALUES
  ('ddddddd4-0000-0000-0000-000000000101'::uuid, CURRENT_DATE + 1, 'aaaaaaa1-0000-0000-0000-000000000001'::uuid, '10:00'::time, '11:00'::time, 0),
  ('ddddddd4-0000-0000-0000-000000000102'::uuid, CURRENT_DATE + 1, 'aaaaaaa1-0000-0000-0000-000000000002'::uuid, '13:00'::time, '14:30'::time, 1),
  ('ddddddd4-0000-0000-0000-000000000103'::uuid, CURRENT_DATE + 2, 'aaaaaaa1-0000-0000-0000-000000000003'::uuid, '10:00'::time, '11:30'::time, 0),
  ('ddddddd4-0000-0000-0000-000000000104'::uuid, CURRENT_DATE + 3, 'aaaaaaa1-0000-0000-0000-000000000004'::uuid, '11:00'::time, '12:30'::time, 1),
  ('ddddddd4-0000-0000-0000-000000000105'::uuid, CURRENT_DATE + 4, 'aaaaaaa1-0000-0000-0000-000000000005'::uuid, '16:00'::time, '16:40'::time, 0),
  ('ddddddd4-0000-0000-0000-000000000106'::uuid, CURRENT_DATE + 6, 'aaaaaaa1-0000-0000-0000-000000000006'::uuid, '10:00'::time, '11:00'::time, 1),
  ('ddddddd4-0000-0000-0000-000000000107'::uuid, CURRENT_DATE + 7, 'aaaaaaa1-0000-0000-0000-000000000001'::uuid, '12:00'::time, '13:30'::time, 0)
) AS v("Id","Date","UserId","StartTime","EndTime","Status")
JOIN "Schedules" s ON s."Date" = v."Date"
WHERE NOT EXISTS (SELECT 1 FROM "Appointments" a WHERE a."Id" = v."Id");

-- Снимки услуг для будущих записей
INSERT INTO "AppointmentOffering" ("AppointmentId", "OfferingId", "Price", "Price_Currency", "Duration")
SELECT a."Id", o."Id", o."Price", o."Price_Currency", o."Duration"
FROM "Appointments" a
JOIN "Schedules" s ON s."Id" = a."ScheduleId"
JOIN (VALUES
  (CURRENT_DATE + 1, '10:00'::time, 'bbbbbbb2-0000-0000-0000-000000000001'::uuid),
  (CURRENT_DATE + 1, '13:00'::time, 'bbbbbbb2-0000-0000-0000-000000000005'::uuid),
  (CURRENT_DATE + 2, '10:00'::time, 'bbbbbbb2-0000-0000-0000-000000000003'::uuid),
  (CURRENT_DATE + 3, '11:00'::time, 'bbbbbbb2-0000-0000-0000-000000000004'::uuid),
  (CURRENT_DATE + 4, '16:00'::time, 'bbbbbbb2-0000-0000-0000-000000000008'::uuid),
  (CURRENT_DATE + 6, '10:00'::time, 'bbbbbbb2-0000-0000-0000-000000000006'::uuid),
  (CURRENT_DATE + 7, '12:00'::time, 'bbbbbbb2-0000-0000-0000-000000000002'::uuid)
) AS av("Date","StartTime","OfferingId")
  ON av."Date" = s."Date" AND av."StartTime" = a."StartTime"
JOIN "Offerings" o ON o."Id" = av."OfferingId"
WHERE NOT EXISTS (
  SELECT 1 FROM "AppointmentOffering" ao WHERE ao."AppointmentId" = a."Id"
);
