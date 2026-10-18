"use client";

import {Alert, Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Paper, Stack, TextField} from "@mui/material";
import {useEffect, useState} from "react";
import Calendar from "@/components/booking/Calendar";
import ServicePicker from "@/components/booking/ServicePicker";
import TimeGrid from "@/components/booking/TimeGrid";
import {ApiError, appointmentsApi, offeringsApi, schedulesApi} from "@/lib/api";
import {durationToMinutes, formatDayLabel, formatPrice, formatTimeOnly} from "@/lib/format";
import {buildSlots} from "@/lib/slots";
import {cardSx, serviceZoneBorder} from "@/lib/theme";
import type {OfferingResponse, TimeInterval} from "@/lib/types";

function Loader() {
    return <Stack sx={{alignItems: "center", py: 2}}><CircularProgress size={22}/></Stack>;
}

export default function CreateAppointmentSection() {
    const [phone, setPhone] = useState("");
    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [needRegister, setNeedRegister] = useState(false);
    const [offerings, setOfferings] = useState<OfferingResponse[]>([]);
    const [selectedIds, setSelectedIds] = useState<string[]>([]);
    const [dates, setDates] = useState<string[]>([]);
    const [datesLoading, setDatesLoading] = useState(false);
    const [date, setDate] = useState<string | null>(null);
    const [intervals, setIntervals] = useState<TimeInterval[]>([]);
    const [slotsLoading, setSlotsLoading] = useState(false);
    const [time, setTime] = useState<string | null>(null);
    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [created, setCreated] = useState<string | null>(null);

    useEffect(() => {
        offeringsApi.getAll()
            .then(setOfferings)
            .catch(() => setError("Не удалось загрузить услуги"));
    }, []);

    useEffect(() => {
        if (selectedIds.length === 0) return;
        let cancelled = false;
        schedulesApi.getAvailableDates(selectedIds)
            .then(res => {
                if (!cancelled) setDates(res.dates);
            })
            .catch(() => {
                if (!cancelled) setDates([]);
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

    const selected = offerings.filter(o => selectedIds.includes(o.id));
    const totalDuration = selected.reduce((sum, o) => sum + durationToMinutes(o.duration), 0);
    const slots = date ? buildSlots(intervals, totalDuration) : [];

    function toggle(id: string) {
        const next = selectedIds.includes(id)
            ? selectedIds.filter(x => x !== id)
            : [...selectedIds, id];
        setSelectedIds(next);
        setDates([]);
        setDate(null);
        setTime(null);
        setIntervals([]);
        setDatesLoading(next.length > 0);
    }

    function selectDate(next: string) {
        if (next === date) return;
        setDate(next);
        setTime(null);
        setIntervals([]);
        setSlotsLoading(true);
    }

    async function submit() {
        if (!date || !time) return;
        setSubmitting(true);
        setError(null);
        setCreated(null);
        try {
            const res = await appointmentsApi.createForUser({
                phone,
                firstName: firstName.trim() || undefined,
                lastName: lastName.trim() || undefined,
                date,
                time: `${time}:00`,
                offeringIds: selectedIds,
            });
            setCreated(
                `${phone}: ${formatDayLabel(res.date)}, ` +
                `${formatTimeOnly(res.interval.start)}–${formatTimeOnly(res.interval.end)}, ` +
                `${formatPrice(res.price.value)}`
            );
            setSelectedIds([]);
            setDates([]);
            setDate(null);
            setTime(null);
            setIntervals([]);
            setNeedRegister(false);
            setFirstName("");
            setLastName("");
        } catch (err) {
            if (err instanceof ApiError && err.status === 404 && err.message.includes("не зарегистрирован")) {
                setNeedRegister(true);
            } else {
                setError(err instanceof Error ? err.message : "Не удалось создать запись");
            }
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <Stack spacing={2}>
            {created && <Alert severity="success">Запись создана — {created}</Alert>}
            {error && <Alert severity="error">{error}</Alert>}
            <TextField
                label="Телефон клиента"
                value={phone}
                onChange={e => setPhone(e.target.value)}
                placeholder="+79341234567"
                fullWidth
            />
            <Paper sx={{...cardSx, p: 1.5, border: 3, borderColor: serviceZoneBorder}}>
                <ServicePicker offerings={offerings} selectedIds={selectedIds} onToggle={toggle}/>
            </Paper>
            {datesLoading && <Loader/>}
            {!datesLoading && selectedIds.length > 0 && (
                dates.length === 0
                    ? <Alert severity="info">Нет доступных дат</Alert>
                    : <Paper sx={cardSx}><Calendar availableDates={dates} selected={date} onSelect={selectDate}/></Paper>
            )}
            {date && (slotsLoading
                ? <Loader/>
                : slots.length === 0
                    ? <Alert severity="info">Нет свободных слотов</Alert>
                    : <Paper sx={cardSx}><TimeGrid slots={slots} selected={time} onSelect={setTime}/></Paper>)}
            <Button
                variant="contained"
                size="large"
                disabled={!phone || selectedIds.length === 0 || !date || !time || submitting}
                onClick={submit}
            >
                Создать запись
            </Button>
            <Dialog open={needRegister} onClose={() => setNeedRegister(false)} fullWidth maxWidth="xs">
                <DialogTitle>Новый клиент</DialogTitle>
                <DialogContent>
                    <Stack spacing={2} sx={{pt: 1}}>
                        <DialogContentText>
                            Клиент с телефоном {phone} не зарегистрирован. Введите имя и фамилию — система создаст карточку клиента и запись.
                        </DialogContentText>
                        {error && <Alert severity="error">{error}</Alert>}
                        <TextField
                            label="Имя"
                            value={firstName}
                            onChange={e => setFirstName(e.target.value)}
                            autoFocus
                            fullWidth
                        />
                        <TextField
                            label="Фамилия"
                            value={lastName}
                            onChange={e => setLastName(e.target.value)}
                            fullWidth
                        />
                    </Stack>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setNeedRegister(false)}>Отмена</Button>
                    <Button
                        variant="contained"
                        disabled={submitting || !firstName.trim()}
                        onClick={submit}
                    >
                        Зарегистрировать и создать
                    </Button>
                </DialogActions>
            </Dialog>
        </Stack>
    );
}
