"use client";

import {CheckCircle as CheckCircleIcon} from "@mui/icons-material";
import {Button, Stack, Typography} from "@mui/material";
import type {ReactNode} from "react";
import {useState} from "react";
import BookingSummaryBar from "@/components/booking/BookingSummaryBar";
import Calendar from "@/components/booking/Calendar";
import OfferingList, {type Offering} from "@/components/booking/OfferingList";
import TimeGrid from "@/components/booking/TimeGrid";
import {AppointmentCard, AppointmentStatus, type Appointment} from "@/components/MyAppointmentsList";
import {formatDurationValue, minutesToTime, timeToMinutes, toIsoDate} from "@/lib/format";

const OFFERINGS: Offering[] = [
    {id: 2, title: "Стрижка", category: "Волосы", price: 600, duration: 45},
    {id: 3, title: "Окрашивание", category: "Волосы", price: 4500, duration: 180},
    {id: 4, title: "Укладка", category: "Волосы", price: 1200, duration: 60},
    {id: 1, title: "Маникюр", category: "Ногти", price: 2000, duration: 60},
    {id: 5, title: "Педикюр", category: "Ногти", price: 2500, duration: 90},
];

const FREE_INTERVALS = [
    {start: "10:00", end: "13:30"},
    {start: "14:00", end: "17:30"},
    {start: "18:00", end: "20:00"},
];

const AVAILABLE_DATES: string[] = (() => {
    const dates: string[] = [];
    for (let i = 0; dates.length < 10; i++) {
        const d = new Date();
        d.setDate(d.getDate() + i);
        if (d.getDay() !== 1) dates.push(toIsoDate(d));
    }
    return dates;
})();

function buildSlots(totalDuration: number): string[] {
    const slots: string[] = [];
    for (const iv of FREE_INTERVALS) {
        const start = timeToMinutes(iv.start);
        const end = timeToMinutes(iv.end);
        for (let t = start; t + totalDuration <= end; t += 30) {
            slots.push(minutesToTime(t));
        }
    }
    return slots;
}

function Section({title, children}: {title: string; children: ReactNode}) {
    return (
        <Stack spacing={2}>
            <Typography variant="h6">{title}</Typography>
            {children}
        </Stack>
    );
}

export default function BookingPage() {
    const [selectedIds, setSelectedIds] = useState<number[]>([]);
    const [date, setDate] = useState<string | null>(null);
    const [time, setTime] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);
    const [booked, setBooked] = useState<{date: string; time: string} | null>(null);

    const selectedOfferings = OFFERINGS.filter(o => selectedIds.includes(o.id));
    const total = selectedOfferings.reduce((sum, o) => sum + o.price, 0);
    const totalDuration = selectedOfferings.reduce((sum, o) => sum + o.duration, 0);
    const slots = date ? buildSlots(totalDuration) : [];

    function toggleOffering(id: number) {
        setSelectedIds(prev => prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]);
        setDate(null);
        setTime(null);
    }

    function submit() {
        if (!date || !time) return;
        setLoading(true);
        setTimeout(() => {
            setLoading(false);
            setBooked({date, time});
        }, 600);
    }

    function reset() {
        setSelectedIds([]);
        setDate(null);
        setTime(null);
        setBooked(null);
    }

    if (booked) {
        const start = `${booked.date}T${booked.time}:00`;
        const end = `${booked.date}T${minutesToTime(timeToMinutes(booked.time) + totalDuration)}:00`;
        const appointment: Appointment = {
            id: 1,
            start,
            end,
            status: AppointmentStatus.Pending,
            services: selectedOfferings.map(o => ({id: o.id, name: o.title, price: o.price})),
        };
        return (
            <>
                <h1>Записаться</h1>
                <Stack spacing={3}>
                    <Stack spacing={1} sx={{alignItems: "center", pt: 4}}>
                        <CheckCircleIcon color="success" sx={{fontSize: 56}}/>
                        <Typography variant="h6">Вы записаны!</Typography>
                    </Stack>
                    <AppointmentCard appointment={appointment} muted={false}/>
                    <Stack direction="row" spacing={2} sx={{justifyContent: "center", pb: 4}}>
                        <Button variant="contained" href="/my-appointments">Мои записи</Button>
                        <Button variant="text" onClick={reset}>Записаться ещё раз</Button>
                    </Stack>
                </Stack>
            </>
        );
    }

    return (
        <>
            <h1>Записаться</h1>
            <Stack spacing={4} sx={{pb: 20}}>
                <Section title="Услуги">
                    <OfferingList offerings={OFFERINGS} selectedIds={selectedIds} onToggle={toggleOffering}/>
                </Section>
                <Section title="Дата">
                    {selectedIds.length === 0
                        ? <Typography color="text.secondary">Сначала выберите услуги</Typography>
                        : <Calendar availableDates={AVAILABLE_DATES} selected={date} onSelect={setDate}/>}
                </Section>
                <Section title="Время">
                    {!date
                        ? <Typography color="text.secondary">Выберите дату</Typography>
                        : slots.length === 0
                            ? <Typography color="text.secondary">Нет свободных слотов</Typography>
                            : <TimeGrid slots={slots} selected={time} onSelect={setTime}/>}
                </Section>
            </Stack>
            {selectedIds.length > 0 && (
                <BookingSummaryBar
                    total={total}
                    durationLabel={formatDurationValue(totalDuration)}
                    disabled={!date || !time}
                    loading={loading}
                    onSubmit={submit}
                />
            )}
        </>
    );
}
