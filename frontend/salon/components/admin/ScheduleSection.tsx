"use client";

import {Alert, Button, Checkbox, Chip, CircularProgress, FormControlLabel, Paper, Stack, TextField, Typography} from "@mui/material";
import {useEffect, useState} from "react";
import {schedulesApi} from "@/lib/api";
import {formatDayLabel, formatTimeOnly} from "@/lib/format";
import {cardSx} from "@/lib/theme";
import type {ScheduleResponse} from "@/lib/types";

type Form = {
    date: string;
    workStart: string;
    workEnd: string;
    breakOn: boolean;
    breakStart: string;
    breakEnd: string;
};

const EMPTY_FORM: Form = {
    date: "",
    workStart: "10:00",
    workEnd: "20:00",
    breakOn: false,
    breakStart: "13:00",
    breakEnd: "14:00",
};

export default function ScheduleSection() {
    const [schedules, setSchedules] = useState<ScheduleResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [form, setForm] = useState<Form>(EMPTY_FORM);

    function load() {
        return schedulesApi.getAll()
            .then(setSchedules)
            .catch(() => setError("Не удалось загрузить расписание"))
            .finally(() => setLoading(false));
    }

    useEffect(() => {
        load();
    }, []);

    async function create() {
        setBusy(true);
        setError(null);
        try {
            await schedulesApi.create({
                date: form.date,
                workInterval: {start: form.workStart, end: form.workEnd},
                breakInterval: form.breakOn ? {start: form.breakStart, end: form.breakEnd} : null,
            });
            setForm(f => ({...f, date: ""}));
            await load();
        } catch (err) {
            setError(err instanceof Error ? err.message : "Не удалось создать день");
        } finally {
            setBusy(false);
        }
    }

    async function changeDay(schedule: ScheduleResponse, isWorking: boolean) {
        setBusy(true);
        setError(null);
        try {
            await schedulesApi.change({
                id: schedule.id,
                isWorking,
                workInterval: schedule.workInterval,
                breakInterval: schedule.breakInterval,
            });
            await load();
        } catch (err) {
            setError(err instanceof Error ? err.message : "Не удалось изменить день");
        } finally {
            setBusy(false);
        }
    }

    return (
        <Stack spacing={2}>
            {error && <Alert severity="error">{error}</Alert>}
            <Paper sx={cardSx}>
                <Stack spacing={2}>
                    <Typography variant="overline" sx={{color: "text.secondary", letterSpacing: "0.12em"}}>
                        Рабочий день
                    </Typography>
                    <TextField
                        label="Дата"
                        type="date"
                        value={form.date}
                        onChange={e => setForm(f => ({...f, date: e.target.value}))}
                        slotProps={{inputLabel: {shrink: true}}}
                        fullWidth
                    />
                    <Stack direction="row" spacing={2}>
                        <TextField
                            label="Начало"
                            type="time"
                            value={form.workStart}
                            onChange={e => setForm(f => ({...f, workStart: e.target.value}))}
                            slotProps={{inputLabel: {shrink: true}}}
                            fullWidth
                        />
                        <TextField
                            label="Конец"
                            type="time"
                            value={form.workEnd}
                            onChange={e => setForm(f => ({...f, workEnd: e.target.value}))}
                            slotProps={{inputLabel: {shrink: true}}}
                            fullWidth
                        />
                    </Stack>
                    <FormControlLabel
                        control={
                            <Checkbox
                                checked={form.breakOn}
                                onChange={e => setForm(f => ({...f, breakOn: e.target.checked}))}
                            />
                        }
                        label="Перерыв"
                    />
                    {form.breakOn && (
                        <Stack direction="row" spacing={2}>
                            <TextField
                                label="Перерыв с"
                                type="time"
                                value={form.breakStart}
                                onChange={e => setForm(f => ({...f, breakStart: e.target.value}))}
                                slotProps={{inputLabel: {shrink: true}}}
                                fullWidth
                            />
                            <TextField
                                label="Перерыв до"
                                type="time"
                                value={form.breakEnd}
                                onChange={e => setForm(f => ({...f, breakEnd: e.target.value}))}
                                slotProps={{inputLabel: {shrink: true}}}
                                fullWidth
                            />
                        </Stack>
                    )}
                    <Button
                        variant="contained"
                        disabled={!form.date || busy}
                        onClick={create}
                    >
                        Сохранить день
                    </Button>
                </Stack>
            </Paper>

            <Stack spacing={1.5}>
                <Typography variant="overline" sx={{color: "text.secondary", letterSpacing: "0.12em"}}>
                    Ближайшие дни
                </Typography>
                {loading && <Stack sx={{alignItems: "center", py: 2}}><CircularProgress size={22}/></Stack>}
                {!loading && schedules.length === 0 && (
                    <Typography variant="body2" color="text.secondary">Расписания нет</Typography>
                )}
                {schedules.map(s => (
                    <Paper key={s.id} sx={cardSx}>
                        <Stack spacing={1}>
                            <Stack direction="row" spacing={1} sx={{alignItems: "center"}}>
                                <Typography variant="subtitle1" sx={{fontWeight: 650}}>
                                    {formatDayLabel(s.date)}
                                </Typography>
                                <Chip
                                    size="small"
                                    label={s.isWorking ? "Рабочий" : "Выходной"}
                                    color={s.isWorking ? "success" : "default"}
                                    variant={s.isWorking ? "filled" : "outlined"}
                                />
                            </Stack>
                            {s.isWorking ? (
                                <Typography variant="body2" color="text.secondary">
                                    {formatTimeOnly(s.workInterval.start)}–{formatTimeOnly(s.workInterval.end)}
                                    {s.breakInterval &&
                                        ` · перерыв ${formatTimeOnly(s.breakInterval.start)}–${formatTimeOnly(s.breakInterval.end)}`}
                                </Typography>
                            ) : (
                                <Typography variant="body2" color="text.secondary">Запись недоступна</Typography>
                            )}
                            <Stack direction="row" spacing={1}>
                                <Button size="small" variant="outlined" disabled={busy || s.isWorking} onClick={() => changeDay(s, true)}>
                                    Сделать рабочим
                                </Button>
                                <Button size="small" variant="outlined" color="error" disabled={busy || !s.isWorking} onClick={() => changeDay(s, false)}>
                                    Сделать выходным
                                </Button>
                            </Stack>
                        </Stack>
                    </Paper>
                ))}
            </Stack>
        </Stack>
    );
}
