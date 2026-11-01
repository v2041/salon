"use client";

import {Alert, Button, ButtonBase, Checkbox, Chip, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle, Divider, FormControlLabel, Paper, Stack, Typography} from "@mui/material";
import {useEffect, useState} from "react";
import {schedulesApi} from "@/lib/api";
import DateField from "@/components/admin/DateField";
import TimeField from "@/components/admin/TimeField";
import {formatDayLabel, formatDurationValue, formatPrice, formatTimeOnly, parseRuDate, parseRuTime, timeToMinutes} from "@/lib/format";
import {cardSx} from "@/lib/theme";
import {AppointmentStatus} from "@/lib/types";
import type {ScheduleAppointmentItem, ScheduleResponse, TimeInterval} from "@/lib/types";

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

const STATUS_META: Record<AppointmentStatus, {label: string; color: "warning" | "success" | "info" | "default" | "error"}> = {
    [AppointmentStatus.Pending]: {label: "Ожидает", color: "warning"},
    [AppointmentStatus.Confirmed]: {label: "Подтверждена", color: "success"},
    [AppointmentStatus.Completed]: {label: "Завершена", color: "info"},
    [AppointmentStatus.Canceled]: {label: "Отменена", color: "error"},
    [AppointmentStatus.Missed]: {label: "Пропущена", color: "error"},
    [AppointmentStatus.Rejected]: {label: "Отклонена", color: "error"},
};

function appointmentMinutes(appointment: ScheduleAppointmentItem) {
    return timeToMinutes(appointment.interval.end) - timeToMinutes(appointment.interval.start);
}

function dayLoad(appointments: ScheduleAppointmentItem[]) {
    const active = appointments.filter(a =>
        a.status === AppointmentStatus.Pending || a.status === AppointmentStatus.Confirmed
    );
    return {
        count: appointments.length,
        busyMinutes: active.reduce((sum, a) => sum + appointmentMinutes(a), 0),
    };
}

export default function ScheduleSection() {
    const [schedules, setSchedules] = useState<ScheduleResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [form, setForm] = useState<Form>(EMPTY_FORM);
    const [editing, setEditing] = useState<ScheduleResponse | null>(null);
    const [editForm, setEditForm] = useState<EditForm>(EMPTY_EDIT);
    const [selected, setSelected] = useState<{date: string; appointment: ScheduleAppointmentItem} | null>(null);

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
                {schedules.map(s => {
                    const load = dayLoad(s.appointments);
                    return (
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
                                <Typography variant="body2" color="text.secondary">
                                    Записей: {load.count}
                                    {load.busyMinutes > 0 && ` · занято ${formatDurationValue(load.busyMinutes)}`}
                                </Typography>
                                <Stack spacing={0.75}>
                                    {s.appointments.length === 0 && (
                                        <Typography variant="body2" color="text.secondary">Записей нет</Typography>
                                    )}
                                    {s.appointments.map(a => {
                                        const status = STATUS_META[a.status];
                                        return (
                                            <ButtonBase
                                                key={a.id}
                                                onClick={() => setSelected({date: s.date, appointment: a})}
                                                sx={{
                                                    width: "100%",
                                                    justifyContent: "space-between",
                                                    gap: 1,
                                                    p: 1,
                                                    borderRadius: 2,
                                                    border: 1,
                                                    borderColor: "divider",
                                                }}
                                            >
                                                <Stack sx={{alignItems: "flex-start"}}>
                                                    <Typography variant="body2" sx={{fontWeight: 600}}>
                                                        {formatTimeOnly(a.interval.start)}–{formatTimeOnly(a.interval.end)} · {a.userName}
                                                    </Typography>
                                                    <Typography variant="caption" color="text.secondary">
                                                        {formatPrice(a.price.value)}
                                                    </Typography>
                                                </Stack>
                                                <Chip size="small" label={status.label} color={status.color} variant="outlined"/>
                                            </ButtonBase>
                                        );
                                    })}
                                </Stack>
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
                    );
                })}
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
            <Dialog open={selected !== null} onClose={() => setSelected(null)} fullWidth maxWidth="xs">
                <DialogTitle>Запись</DialogTitle>
                <DialogContent>
                    {selected && (
                        <Stack spacing={1.5} sx={{pt: 1}}>
                            <Stack direction="row" sx={{justifyContent: "space-between", alignItems: "center", gap: 1}}>
                                <Typography variant="subtitle1" sx={{fontWeight: 650}}>
                                    {formatDayLabel(selected.date)}
                                </Typography>
                                <Chip
                                    size="small"
                                    label={STATUS_META[selected.appointment.status].label}
                                    color={STATUS_META[selected.appointment.status].color}
                                    variant="outlined"
                                />
                            </Stack>
                            <Typography variant="body2" color="text.secondary">
                                {formatTimeOnly(selected.appointment.interval.start)}–{formatTimeOnly(selected.appointment.interval.end)}
                                {" · "}
                                {formatDurationValue(appointmentMinutes(selected.appointment))}
                            </Typography>
                            <Divider/>
                            <Stack direction="row" sx={{justifyContent: "space-between", alignItems: "baseline", gap: 1}}>
                                <Typography variant="body2" color="text.secondary">Клиент</Typography>
                                <Typography variant="body2">{selected.appointment.userName}</Typography>
                            </Stack>
                            <Stack direction="row" sx={{justifyContent: "space-between", alignItems: "baseline", gap: 1}}>
                                <Typography variant="body2" color="text.secondary">Стоимость</Typography>
                                <Typography variant="subtitle1" sx={{fontWeight: 700}}>
                                    {formatPrice(selected.appointment.price.value)}
                                </Typography>
                            </Stack>
                        </Stack>
                    )}
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setSelected(null)}>Закрыть</Button>
                </DialogActions>
            </Dialog>
        </Stack>
    );
}
