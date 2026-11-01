"use client";

import {CheckCircle as CheckCircleIcon, Place as PlaceIcon, Phone as PhoneIcon} from "@mui/icons-material";
import {Alert, Box, Button, CircularProgress, Paper, Stack, Typography} from "@mui/material";
import {Suspense, useState} from "react";
import {useSearchParams} from "next/navigation";
import Calendar from "@/components/booking/Calendar";
import ServicePicker from "@/components/booking/ServicePicker";
import TimeGrid from "@/components/booking/TimeGrid";
import {appointmentsApi} from "@/lib/api";
import {formatDayLabel, formatDurationValue, formatPrice, formatTimeOnly, timeToMinutes} from "@/lib/format";
import {accentInk, cardSx, darkSurface, serviceZoneBorder} from "@/lib/theme";
import {useBookingFlow} from "@/lib/useBookingFlow";
import type {Money} from "@/lib/types";

const ADDRESS = process.env.NEXT_PUBLIC_SALON_ADDRESS;
const PHONE = process.env.NEXT_PUBLIC_SALON_PHONE;

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
    return (
        <Suspense fallback={<Loader/>}>
            <BookingContent/>
        </Suspense>
    );
}

function BookingContent() {
    const searchParams = useSearchParams();
    const preselectedId = searchParams.get("offeringId");
    const flow = useBookingFlow(preselectedId ? [preselectedId] : []);
    const [step, setStep] = useState<"services" | "time">("services");
    const [submitting, setSubmitting] = useState(false);
    const [booked, setBooked] = useState<Booked | null>(null);

    async function submit() {
        if (!flow.date || !flow.time) return;
        setSubmitting(true);
        flow.setError(null);
        try {
            const created = await appointmentsApi.create({
                date: flow.date,
                time: `${flow.time}:00`,
                offeringIds: flow.selectedIds,
            });
            setBooked({
                date: created.date,
                start: created.interval.start,
                end: created.interval.end,
                price: created.price,
            });
        } catch (err) {
            flow.setError(err instanceof Error ? err.message : "Не удалось создать запись");
        } finally {
            setSubmitting(false);
        }
    }

    function reset() {
        flow.reset();
        setBooked(null);
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

            {flow.error && <Alert severity="error">{flow.error}</Alert>}

            {step === "services" && (
                <>
                    <Paper sx={{...cardSx, p: 1.5, border: 3, borderColor: serviceZoneBorder}}>
                        {flow.offeringsLoading
                            ? <Loader/>
                            : <ServicePicker offerings={flow.offerings} selectedIds={flow.selectedIds} onToggle={flow.toggle}/>}
                    </Paper>
                    <Paper sx={darkSx}>
                        <Stack direction="row" spacing={2} sx={{alignItems: "center"}}>
                            <Box sx={{flex: 1, minWidth: 0}}>
                                <Typography variant="caption" sx={{opacity: 0.65}}>
                                    Выбрано: {flow.selected.length} · {formatDurationValue(flow.totalDuration)}
                                </Typography>
                                <Typography variant="h6">
                                    {formatPrice(flow.selected.reduce((sum, o) => sum + o.price.value, 0))}
                                </Typography>
                            </Box>
                            <Button
                                variant="contained"
                                color="secondary"
                                disabled={flow.selected.length === 0}
                                onClick={() => setStep("time")}
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
                                <Typography variant="subtitle1" sx={{fontWeight: 650}}>
                                    {formatPrice(flow.selected.reduce((sum, o) => sum + o.price.value, 0))}
                                </Typography>
                                <Typography variant="caption" color="text.secondary">
                                    {flow.selected.map(o => o.title).join(", ")} · {formatDurationValue(flow.totalDuration)}
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
                            {flow.datesLoading
                                ? <Loader/>
                                : flow.dates.length === 0
                                    ? <Alert severity="info">Нет доступных дат</Alert>
                                    : <Calendar availableDates={flow.dates} selected={flow.date} onSelect={flow.selectDate}/>}
                        </Stack>
                    </Paper>

                    {flow.date && (
                        <Paper sx={cardSx}>
                            <Stack spacing={2}>
                                <Typography variant="overline" sx={{color: "text.secondary", letterSpacing: "0.12em"}}>
                                    Время
                                </Typography>
                                {flow.slotsLoading
                                    ? <Loader/>
                                    : flow.slots.length === 0
                                        ? <Alert severity="info">Нет свободных слотов</Alert>
                                        : <TimeGrid slots={flow.slots} selected={flow.time} onSelect={flow.setTime}/>}
                            </Stack>
                        </Paper>
                    )}

                    <Button
                        variant="contained"
                        size="large"
                        disabled={!flow.date || !flow.time || submitting}
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
    if (!ADDRESS && !PHONE) return null;
    return (
        <Paper sx={darkSx}>
            <Stack spacing={1.25}>
                <Typography variant="overline" sx={{opacity: 0.6, letterSpacing: "0.14em"}}>Контакты</Typography>
                {ADDRESS && (
                    <Stack direction="row" spacing={1.25} sx={{alignItems: "center"}}>
                        <PlaceIcon fontSize="small" sx={{color: "secondary.main"}}/>
                        <Typography variant="body2">{ADDRESS}</Typography>
                    </Stack>
                )}
                {PHONE && (
                    <Stack direction="row" spacing={1.25} sx={{alignItems: "center"}}>
                        <PhoneIcon fontSize="small" sx={{color: "secondary.main"}}/>
                        <Typography variant="body2" component="a" href={`tel:${PHONE}`} sx={{color: "inherit"}}>
                            {PHONE}
                        </Typography>
                    </Stack>
                )}
            </Stack>
        </Paper>
    );
}
