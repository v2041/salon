"use client";

import {Alert, Button, Checkbox, Chip, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle, FormControlLabel, Paper, Stack, Typography} from "@mui/material";
import {useEffect, useState} from "react";
import {schedulesApi} from "@/lib/api";
import DateField from "@/components/admin/DateField";
import TimeField from "@/components/admin/TimeField";
import {formatDayLabel, formatTimeOnly, parseRuDate, parseRuTime} from "@/lib/format";
import {cardSx} from "@/lib/theme";
import type {ScheduleResponse, TimeInterval} from "@/lib/types";

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

type EditForm = {
    start: string;
    end: string;
    breakOn: boolean;
    breakStart: string;
    breakEnd: string;
};

const EMPTY_EDIT: EditForm = {start: "10:00", end: "20:00", breakOn: false, breakStart: "13:00", breakEnd: "14:00"};

export default function ScheduleSection() {
    const [schedules, setSchedules] = useState<ScheduleResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [form, setForm] = useState<Form>(EMPTY_FORM);
    const [editing, setEditing] = useState<ScheduleResponse | null>(null);
    const [editForm, setEditForm] = useState<EditForm>(EMPTY_EDIT);

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
        const date = parseRuDate(form.date);
        const workStart = parseRuTime(form.workStart);
        const workEnd = parseRuTime(form.workEnd);
        if (!date || !workStart || !workEnd) {
            setError("Проверьте формат: дата — дд.мм.гггг, время — чч:мм");
            return;
        }
        let breakInterval: TimeInterval | null = null;
        if (form.breakOn) {
            const breakStart = parseRuTime(form.breakStart);
            const breakEnd = parseRuTime(form.breakEnd);
            if (!breakStart || !breakEnd) {
                setError("Проверьте формат времени перерыва — чч:мм");
                return;
            }
            breakInterval = {start: breakStart, end: breakEnd};
        }
        setBusy(true);
        setError(null);
        try {
            await schedulesApi.create({date, workInterval: {start: workStart, end: workEnd}, breakInterval});
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

    function openEdit(schedule: ScheduleResponse) {
        setEditing(schedule);
        setEditForm({
            start: formatTimeOnly(schedule.workInterval.start),
            end: formatTimeOnly(schedule.workInterval.end),
            breakOn: schedule.breakInterval !== null,
            breakStart: schedule.breakInterval ? formatTimeOnly(schedule.breakInterval.start) : "13:00",
            breakEnd: schedule.breakInterval ? formatTimeOnly(schedule.breakInterval.end) : "14:00",
        });
    }

    async function saveEdit() {
        if (!editing) return;
        const start = parseRuTime(editForm.start);
        const end = parseRuTime(editForm.end);
        if (!start || !end) {
            setError("Проверьте время работы — чч:мм");
            return;
        }
        let breakInterval: TimeInterval | null = null;
        if (editForm.breakOn) {
            const breakStart = parseRuTime(editForm.breakStart);
            const breakEnd = parseRuTime(editForm.breakEnd);
            if (!breakStart || !breakEnd) {
                setError("Проверьте время перерыва — чч:мм");
                return;
            }
            breakInterval = {start: breakStart, end: breakEnd};
        }
        setBusy(true);
        setError(null);
        try {
            await schedulesApi.change({
                id: editing.id,
                isWorking: editing.isWorking,
                workInterval: {start, end},
                breakInterval,
            });
            setEditing(null);
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
                    <DateField
                        label="Дата"
                        value={form.date}
                        onChange={value => setForm(f => ({...f, date: value}))}
                        disablePast
                    />
                    <Stack direction="row" spacing={2}>
                        <TimeField
                            label="Начало"
                            value={form.workStart}
                            onChange={value => setForm(f => ({...f, workStart: value}))}
                        />
                        <TimeField
                            label="Конец"
                            value={form.workEnd}
                            onChange={value => setForm(f => ({...f, workEnd: value}))}
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
                            <TimeField
                                label="Перерыв с"
                                value={form.breakStart}
                                onChange={value => setForm(f => ({...f, breakStart: value}))}
                            />
                            <TimeField
                                label="Перерыв до"
                                value={form.breakEnd}
                                onChange={value => setForm(f => ({...f, breakEnd: value}))}
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
                                <Button size="small" variant="outlined" disabled={busy} onClick={() => openEdit(s)}>
                                    Изменить
                                </Button>
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
            <Dialog open={Boolean(editing)} onClose={() => setEditing(null)} fullWidth maxWidth="xs">
                <DialogTitle>Изменить день</DialogTitle>
                <DialogContent>
                    <Stack spacing={2} sx={{pt: 1}}>
                        {editing && (
                            <Typography variant="subtitle1" sx={{fontWeight: 650}}>
                                {formatDayLabel(editing.date)}
                            </Typography>
                        )}
                        {error && <Alert severity="error">{error}</Alert>}
                        <Stack direction="row" spacing={2}>
                            <TimeField
                                label="Начало"
                                value={editForm.start}
                                onChange={value => setEditForm(f => ({...f, start: value}))}
                            />
                            <TimeField
                                label="Конец"
                                value={editForm.end}
                                onChange={value => setEditForm(f => ({...f, end: value}))}
                            />
                        </Stack>
                        <FormControlLabel
                            control={
                                <Checkbox
                                    checked={editForm.breakOn}
                                    onChange={e => setEditForm(f => ({...f, breakOn: e.target.checked}))}
                                />
                            }
                            label="Перерыв"
                        />
                        {editForm.breakOn && (
                            <Stack direction="row" spacing={2}>
                                <TimeField
                                    label="Перерыв с"
                                    value={editForm.breakStart}
                                    onChange={value => setEditForm(f => ({...f, breakStart: value}))}
                                />
                                <TimeField
                                    label="Перерыв до"
                                    value={editForm.breakEnd}
                                    onChange={value => setEditForm(f => ({...f, breakEnd: value}))}
                                />
                            </Stack>
                        )}
                    </Stack>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setEditing(null)}>Отмена</Button>
                    <Button variant="contained" disabled={busy} onClick={saveEdit}>Сохранить</Button>
                </DialogActions>
            </Dialog>
        </Stack>
    );
}
