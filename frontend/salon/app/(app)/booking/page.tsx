"use client";

import {CheckCircle as CheckCircleIcon, Place as PlaceIcon, Phone as PhoneIcon} from "@mui/icons-material";
import {Alert, Box, Button, CircularProgress, Paper, Stack, Typography} from "@mui/material";
import {useEffect, useState} from "react";
import Calendar from "@/components/booking/Calendar";
import ServicePicker from "@/components/booking/ServicePicker";
import TimeGrid from "@/components/booking/TimeGrid";
import {appointmentsApi, offeringsApi, schedulesApi} from "@/lib/api";
import {durationToMinutes, formatDayLabel, formatDurationValue, formatPrice, formatTimeOnly, timeToMinutes} from "@/lib/format";
import {accentInk, cardSx, darkSurface, serviceZoneBorder} from "@/lib/theme";
import {buildSlots} from "@/lib/slots";
import type {Money, OfferingResponse, TimeInterval} from "@/lib/types";

const CONTACTS = {address: "ул. Примерная, д. 1", phone: "+7 (999) 123-45-67"};

const darkSx = {
    p: 2,
    borderRadius: "20px",
    border: "none",
    color: "#FBF3F2",
    backgroundColor: darkSurface,
};

function Loader() {
    return <Stack sx={{alignItems: "center", py: 3}}><CircularProgress size={22}/></Stack>;
}

function StepDots({step}: {step: "services" | "time"}) {
    const active = step === "services" ? 0 : 1;
    return (
        <Stack direction="row" spacing={0.75} sx={{alignItems: "center"}}>
            {[0, 1].map(i => (
                <Box
                    key={i}
                    sx={{
                        height: 4,
                        width: 34,
                        borderRadius: 2,
                        bgcolor: i <= active ? "primary.main" : "divider",
                        transition: "background-color .2s",
                    }}
                />
            ))}
            <Typography variant="caption" sx={{ml: 0.75, color: "text.secondary"}}>
                Шаг {active + 1} из 2
            </Typography>
        </Stack>
    );
}

type Booked = {date: string; start: string; end: string; price: Money};

export default function BookingPage() {
    const [offerings, setOfferings] = useState<OfferingResponse[]>([]);
    const [offeringsLoading, setOfferingsLoading] = useState(true);
    const [step, setStep] = useState<"services" | "time">("services");
    const [selectedIds, setSelectedIds] = useState<string[]>([]);
    const [dates, setDates] = useState<string[]>([]);
    const [datesLoading, setDatesLoading] = useState(false);
    const [date, setDate] = useState<string | null>(null);
    const [intervals, setIntervals] = useState<TimeInterval[]>([]);
    const [slotsLoading, setSlotsLoading] = useState(false);
    const [time, setTime] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [booked, setBooked] = useState<Booked | null>(null);

    useEffect(() => {
        offeringsApi.getAll()
            .then(setOfferings)
            .catch(() => setError("Не удалось загрузить услуги"))
            .finally(() => setOfferingsLoading(false));
    }, []);

    useEffect(() => {
        if (step !== "time") return;
        let cancelled = false;
        schedulesApi.getAvailableDates(selectedIds)
            .then(res => { if (!cancelled) setDates(res.dates); })
            .catch(() => { if (!cancelled) setDates([]); })
            .finally(() => { if (!cancelled) setDatesLoading(false); });
        return () => { cancelled = true; };
    }, [step, selectedIds]);

    useEffect(() => {
        if (!date) return;
        let cancelled = false;
        schedulesApi.getFreeTime(date)
            .then(res => { if (!cancelled) setIntervals(res.freeIntervals); })
            .catch(() => { if (!cancelled) setIntervals([]); })
            .finally(() => { if (!cancelled) setSlotsLoading(false); });
        return () => { cancelled = true; };
    }, [date]);

    const selected = offerings.filter(o => selectedIds.includes(o.id));
    const total = selected.reduce((sum, o) => sum + o.price.value, 0);
    const totalDuration = selected.reduce((sum, o) => sum + durationToMinutes(o.duration), 0);
    const slots = date ? buildSlots(intervals, totalDuration) : [];

    function toggle(id: string) {
        setSelectedIds(prev => prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id]);
    }

    function goToTime() {
        setDates([]);
        setDatesLoading(true);
        setDate(null);
        setTime(null);
        setIntervals([]);
        setStep("time");
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
        setError(null);
        try {
            const created = await appointmentsApi.create({date, time: `${time}:00`, offeringIds: selectedIds});
            setBooked({date: created.date, start: created.interval.start, end: created.interval.end, price: created.price});
        } catch (err) {
            setError(err instanceof Error ? err.message : "Не удалось создать запись");
        } finally {
            setSubmitting(false);
        }
    }

    function reset() {
        setSelectedIds([]);
        setDate(null);
        setTime(null);
        setDates([]);
        setIntervals([]);
        setBooked(null);
        setError(null);
        setStep("services");
    }

    if (booked) {
        return (
            <Stack spacing={2.5}>
                <Paper sx={{...cardSx, p: 3}}>
                    <Stack spacing={2} sx={{alignItems: "center", textAlign: "center"}}>
                        <Box sx={{width: 64, height: 64, borderRadius: "50%", bgcolor: "rgba(201,138,147,0.18)", display: "flex", alignItems: "center", justifyContent: "center"}}>
                            <CheckCircleIcon sx={{fontSize: 34, color: accentInk}}/>
                        </Box>
                        <Typography variant="h6">Вы записаны</Typography>
                        <Stack spacing={0.25}>
                            <Typography variant="subtitle1" sx={{fontWeight: 650}}>{formatDayLabel(booked.date)}</Typography>
                            <Typography variant="body2" color="text.secondary">
                                {formatTimeOnly(booked.start)}–{formatTimeOnly(booked.end)} ·{" "}
                                {formatDurationValue(timeToMinutes(booked.end) - timeToMinutes(booked.start))}
                            </Typography>
                        </Stack>
                        <Typography variant="h6">{formatPrice(booked.price.value)}</Typography>
                        <Button variant="contained" fullWidth onClick={reset}>Записаться ещё раз</Button>
                    </Stack>
                </Paper>
                <ContactsCard/>
            </Stack>
        );
    }

    return (
        <Stack spacing={2.5}>
            <Stack spacing={1}>
                <Typography variant="overline" sx={{color: accentInk, letterSpacing: "0.16em"}}>
                    Онлайн-запись
                </Typography>
                <Typography variant="h5">
                    {step === "services" ? "Выберите услуги" : "Выберите время"}
                </Typography>
                <StepDots step={step}/>
            </Stack>

            {error && <Alert severity="error">{error}</Alert>}

            {step === "services" && (
                <>
                    <Paper sx={{...cardSx, p: 1.5, border: 3, borderColor: serviceZoneBorder}}>
                        {offeringsLoading
                            ? <Loader/>
                            : <ServicePicker offerings={offerings} selectedIds={selectedIds} onToggle={toggle}/>}
                    </Paper>
                    <Paper sx={darkSx}>
                        <Stack direction="row" spacing={2} sx={{alignItems: "center"}}>
                            <Box sx={{flex: 1, minWidth: 0}}>
                                <Typography variant="caption" sx={{opacity: 0.65}}>
                                    Выбрано: {selected.length} · {formatDurationValue(totalDuration)}
                                </Typography>
                                <Typography variant="h6">{formatPrice(total)}</Typography>
                            </Box>
                            <Button
                                variant="contained"
                                color="secondary"
                                disabled={selectedIds.length === 0}
                                onClick={goToTime}
                            >
                                Далее
                            </Button>
                        </Stack>
                    </Paper>
                </>
            )}

            {step === "time" && (
                <>
                    <Paper sx={cardSx}>
                        <Stack direction="row" spacing={1} sx={{alignItems: "center"}}>
                            <Box sx={{flex: 1, minWidth: 0}}>
                                <Typography variant="subtitle1" sx={{fontWeight: 650}}>{formatPrice(total)}</Typography>
                                <Typography variant="caption" color="text.secondary">
                                    {selected.map(o => o.title).join(", ")} · {formatDurationValue(totalDuration)}
                                </Typography>
                            </Box>
                            <Button variant="text" size="small" onClick={() => setStep("services")}>Изменить</Button>
                        </Stack>
                    </Paper>

                    <Paper sx={cardSx}>
                        <Stack spacing={2}>
                            <Typography variant="overline" sx={{color: "text.secondary", letterSpacing: "0.12em"}}>
                                Дата
                            </Typography>
                            {datesLoading
                                ? <Loader/>
                                : dates.length === 0
                                    ? <Alert severity="info">Нет доступных дат</Alert>
                                    : <Calendar availableDates={dates} selected={date} onSelect={selectDate}/>}
                        </Stack>
                    </Paper>

                    {date && (
                        <Paper sx={cardSx}>
                            <Stack spacing={2}>
                                <Typography variant="overline" sx={{color: "text.secondary", letterSpacing: "0.12em"}}>
                                    Время
                                </Typography>
                                {slotsLoading
                                    ? <Loader/>
                                    : slots.length === 0
                                        ? <Alert severity="info">Нет свободных слотов</Alert>
                                        : <TimeGrid slots={slots} selected={time} onSelect={setTime}/>}
                            </Stack>
                        </Paper>
                    )}

                    <Button
                        variant="contained"
                        size="large"
                        disabled={!date || !time || submitting}
                        onClick={submit}
                        sx={{py: 1.5}}
                    >
                        Записаться
                    </Button>
                </>
            )}

            <ContactsCard/>
        </Stack>
    );
}

function ContactsCard() {
    return (
        <Paper sx={darkSx}>
            <Stack spacing={1.25}>
                <Typography variant="overline" sx={{opacity: 0.6, letterSpacing: "0.14em"}}>Контакты</Typography>
                <Stack direction="row" spacing={1.25} sx={{alignItems: "center"}}>
                    <PlaceIcon fontSize="small" sx={{color: "secondary.main"}}/>
                    <Typography variant="body2">{CONTACTS.address}</Typography>
                </Stack>
                <Stack direction="row" spacing={1.25} sx={{alignItems: "center"}}>
                    <PhoneIcon fontSize="small" sx={{color: "secondary.main"}}/>
                    <Typography variant="body2" component="a" href={`tel:${CONTACTS.phone}`} sx={{color: "inherit"}}>
                        {CONTACTS.phone}
                    </Typography>
                </Stack>
            </Stack>
        </Paper>
    );
}
