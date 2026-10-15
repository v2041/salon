"use client";

import {CheckCircle as CheckCircleIcon} from "@mui/icons-material";
import {Alert, Button, CircularProgress, Paper, Stack, Typography} from "@mui/material";
import type {ReactNode} from "react";
import {useEffect, useState} from "react";
import BookingSummaryBar from "@/components/booking/BookingSummaryBar";
import Calendar from "@/components/booking/Calendar";
import OfferingList from "@/components/booking/OfferingList";
import TimeGrid from "@/components/booking/TimeGrid";
import {ApiError, appointmentsApi, offeringsApi, schedulesApi} from "@/lib/api";
import {
    durationToMinutes,
    formatDayLabel,
    formatDurationValue,
    formatPrice,
    formatTimeOnly,
    minutesToTime,
    timeToMinutes,
} from "@/lib/format";
import type {Money, OfferingResponse, TimeInterval} from "@/lib/types";

function buildSlots(intervals: TimeInterval[], totalDuration: number): string[] {
    const slots: string[] = [];
    for (const iv of intervals) {
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

function Loader() {
    return (
        <Stack sx={{alignItems: "center", py: 2}}>
            <CircularProgress size={24}/>
        </Stack>
    );
}

function errorMessage(error: unknown, fallback: string) {
    return error instanceof Error ? error.message : fallback;
}

type Booked = {id: string; date: string; start: string; end: string; price: Money};

export default function BookingPage() {
    const [offerings, setOfferings] = useState<OfferingResponse[]>([]);
    const [offeringsLoading, setOfferingsLoading] = useState(true);
    const [offeringsError, setOfferingsError] = useState<string | null>(null);

    const [selectedIds, setSelectedIds] = useState<string[]>([]);
    const [availableDates, setAvailableDates] = useState<string[]>([]);
    const [datesLoading, setDatesLoading] = useState(false);
    const [datesError, setDatesError] = useState<string | null>(null);
    const [date, setDate] = useState<string | null>(null);

    const [intervals, setIntervals] = useState<TimeInterval[]>([]);
    const [slotsLoading, setSlotsLoading] = useState(false);
    const [time, setTime] = useState<string | null>(null);

    const [submitting, setSubmitting] = useState(false);
    const [submitError, setSubmitError] = useState<string | null>(null);
    const [booked, setBooked] = useState<Booked | null>(null);

    useEffect(() => {
        offeringsApi.getAll()
            .then(setOfferings)
            .catch((e: unknown) => setOfferingsError(errorMessage(e, "Не удалось загрузить услуги")))
            .finally(() => setOfferingsLoading(false));
    }, []);

    useEffect(() => {
        if (selectedIds.length === 0) return;
        let cancelled = false;
        schedulesApi.getAvailableDates(selectedIds)
            .then(res => {
                if (!cancelled) setAvailableDates(res.dates);
            })
            .catch((e: unknown) => {
                if (!cancelled) setDatesError(errorMessage(e, "Не удалось загрузить даты"));
            })
            .finally(() => {
                if (!cancelled) setDatesLoading(false);
            });
        return () => {
            cancelled = true;
        };
    }, [selectedIds]);

    useEffect(() => {
        if (!date) return;
        let cancelled = false;
        schedulesApi.getFreeTime(date)
            .then(res => {
                if (!cancelled) setIntervals(res.freeIntervals);
            })
            .catch(() => {
                if (!cancelled) setIntervals([]);
            })
            .finally(() => {
                if (!cancelled) setSlotsLoading(false);
            });
        return () => {
            cancelled = true;
        };
    }, [date]);

    const selectedOfferings = offerings.filter(o => selectedIds.includes(o.id));
    const total = selectedOfferings.reduce((sum, o) => sum + o.price.value, 0);
    const totalDuration = selectedOfferings.reduce((sum, o) => sum + durationToMinutes(o.duration), 0);
    const slots = date ? buildSlots(intervals, totalDuration) : [];

    function toggleOffering(id: string) {
        const next = selectedIds.includes(id) ? selectedIds.filter(x => x !== id) : [...selectedIds, id];
        setSelectedIds(next);
        setDate(null);
        setTime(null);
        setAvailableDates([]);
        setIntervals([]);
        setDatesError(null);
        setDatesLoading(next.length > 0);
    }

    function selectDate(next: string) {
        setDate(next);
        setTime(null);
        setIntervals([]);
        setSlotsLoading(true);
    }

    async function submit() {
        if (!date || !time) return;
        setSubmitting(true);
        setSubmitError(null);
        try {
            const created = await appointmentsApi.create({
                date,
                time: `${time}:00`,
                offeringIds: selectedIds,
            });
            setBooked({
                id: created.id,
                date: created.date,
                start: created.interval.start,
                end: created.interval.end,
                price: created.price,
            });
        } catch (e) {
            if (e instanceof ApiError && e.status === 401) {
                setSubmitError("Войдите, чтобы записаться");
            } else {
                setSubmitError(errorMessage(e, "Не удалось создать запись"));
            }
        } finally {
            setSubmitting(false);
        }
    }

    function reset() {
        setSelectedIds([]);
        setDate(null);
        setTime(null);
        setAvailableDates([]);
        setIntervals([]);
        setDatesError(null);
        setDatesLoading(false);
        setSlotsLoading(false);
        setSubmitError(null);
        setBooked(null);
    }

    if (booked) {
        return (
            <>
                <h1>Записаться</h1>
                <Stack spacing={3}>
                    <Stack spacing={1} sx={{alignItems: "center", pt: 4}}>
                        <CheckCircleIcon color="success" sx={{fontSize: 56}}/>
                        <Typography variant="h6">Вы записаны!</Typography>
                    </Stack>
                    <Paper variant="outlined" elevation={1} sx={{borderRadius: 2, p: 2, borderColor: "text.disabled"}}>
                        <Stack spacing={0.5}>
                            <Typography variant="subtitle1" sx={{fontWeight: 600}}>{formatDayLabel(booked.date)}</Typography>
                            <Typography variant="body2" color="text.secondary">
                                {formatTimeOnly(booked.start)}–{formatTimeOnly(booked.end)} ·{" "}
                                {formatDurationValue(timeToMinutes(booked.end) - timeToMinutes(booked.start))}
                            </Typography>
                            <Typography variant="body2" sx={{fontWeight: 600}}>{formatPrice(booked.price.value)}</Typography>
                        </Stack>
                    </Paper>
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
                    {offeringsLoading
                        ? <Loader/>
                        : offeringsError
                            ? <Alert severity="error">{offeringsError}</Alert>
                            : <OfferingList offerings={offerings} selectedIds={selectedIds} onToggle={toggleOffering}/>}
                </Section>
                <Section title="Дата">
                    {selectedIds.length === 0
                        ? <Typography color="text.secondary">Сначала выберите услуги</Typography>
                        : datesLoading
                            ? <Loader/>
                            : datesError
                                ? <Alert severity="error">{datesError}</Alert>
                                : availableDates.length === 0
                                    ? <Typography color="text.secondary">Нет доступных дат</Typography>
                                    : <Calendar availableDates={availableDates} selected={date} onSelect={selectDate}/>}
                </Section>
                <Section title="Время">
                    {!date
                        ? <Typography color="text.secondary">Выберите дату</Typography>
                        : slotsLoading
                            ? <Loader/>
                            : slots.length === 0
                                ? <Typography color="text.secondary">Нет свободных слотов</Typography>
                                : <TimeGrid slots={slots} selected={time} onSelect={setTime}/>}
                </Section>
                {submitError && <Alert severity="error">{submitError}</Alert>}
            </Stack>
            {selectedIds.length > 0 && (
                <BookingSummaryBar
                    total={total}
                    durationLabel={formatDurationValue(totalDuration)}
                    disabled={!date || !time}
                    loading={submitting}
                    onSubmit={submit}
                />
            )}
        </>
    );
}
