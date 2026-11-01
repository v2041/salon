"use client";

import {Alert, Button, Chip, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, Paper, Select, Stack, TextField, Typography} from "@mui/material";
import {useEffect, useState} from "react";
import {appointmentsApi} from "@/lib/api";
import DateField from "@/components/admin/DateField";
import TimeField from "@/components/admin/TimeField";
import {durationToMinutes, formatDayLabel, formatDurationValue, formatPrice, formatRuDate, formatTimeOnly, minutesToDuration, parseRuDate, parseRuTime, toIsoDate} from "@/lib/format";
import {accentInk, cardSx} from "@/lib/theme";
import {AppointmentStatus} from "@/lib/types";
import type {AdminAppointmentOffering, AdminAppointmentResponse} from "@/lib/types";

const STATUS_META: Record<AppointmentStatus, {label: string; color: "warning" | "success" | "info" | "default" | "error"}> = {
    [AppointmentStatus.Pending]: {label: "Ожидает", color: "warning"},
    [AppointmentStatus.Confirmed]: {label: "Подтверждена", color: "success"},
    [AppointmentStatus.Completed]: {label: "Завершена", color: "info"},
    [AppointmentStatus.Canceled]: {label: "Отменена", color: "error"},
    [AppointmentStatus.Missed]: {label: "Пропущена", color: "error"},
    [AppointmentStatus.Rejected]: {label: "Отклонена", color: "error"},
};

const STATUS_ORDER = [
    AppointmentStatus.Pending,
    AppointmentStatus.Confirmed,
    AppointmentStatus.Completed,
    AppointmentStatus.Canceled,
    AppointmentStatus.Missed,
    AppointmentStatus.Rejected,
];

type Action = {label: string; status: AppointmentStatus; color: "error" | "success"};

const ACTIONS: Record<AppointmentStatus, Action[]> = {
    [AppointmentStatus.Pending]: [
        {label: "Подтвердить", status: AppointmentStatus.Confirmed, color: "success"},
        {label: "Отклонить", status: AppointmentStatus.Rejected, color: "error"},
    ],
    [AppointmentStatus.Confirmed]: [
        {label: "Завершить", status: AppointmentStatus.Completed, color: "success"},
        {label: "Пропуск", status: AppointmentStatus.Missed, color: "error"},
        {label: "Отменить", status: AppointmentStatus.Canceled, color: "error"},
    ],
    [AppointmentStatus.Completed]: [],
    [AppointmentStatus.Canceled]: [],
    [AppointmentStatus.Missed]: [],
    [AppointmentStatus.Rejected]: [],
};

type Filter = AppointmentStatus | "all";

function startOf(a: AdminAppointmentResponse) {
    return new Date(`${a.date}T${a.interval.start}`);
}

function AppointmentCard({
    appointment,
    busy,
    onStatus,
    onEdit,
    onReschedule,
}: {
    appointment: AdminAppointmentResponse;
    busy: boolean;
    onStatus: (id: string, status: AppointmentStatus) => void;
    onEdit: (appointment: AdminAppointmentResponse, offering: AdminAppointmentOffering) => void;
    onReschedule: (appointment: AdminAppointmentResponse) => void;
}) {
    const status = STATUS_META[appointment.status];
    return (
        <Paper sx={cardSx}>
            <Stack direction="row" sx={{justifyContent: "space-between", alignItems: "center", mb: 0.5}}>
                <Typography variant="subtitle1" sx={{fontWeight: 650}}>
                    {formatDayLabel(appointment.date)} · {formatTimeOnly(appointment.interval.start)}–{formatTimeOnly(appointment.interval.end)}
                </Typography>
                <Chip label={status.label} size="small" color={status.color} variant="outlined"/>
            </Stack>
            <Typography variant="body2" color="text.secondary">
                {appointment.userName}
                {appointment.userPhone ? ` · ${appointment.userPhone}` : ""}
            </Typography>
            <Stack spacing={0.75} sx={{mt: 1.5}}>
                {appointment.offerings.map(o => (
                    <Stack
                        key={o.offeringId}
                        direction="row"
                        sx={{alignItems: "center", justifyContent: "space-between", gap: 1}}
                    >
                        <Typography variant="body2" color="text.secondary" sx={{flex: 1}}>
                            {o.title}
                        </Typography>
                        <Typography variant="body2" sx={{whiteSpace: "nowrap"}}>
                            {formatPrice(o.price.value)} · {formatDurationValue(durationToMinutes(o.duration))}
                        </Typography>
                        <Button size="small" disabled={busy} onClick={() => onEdit(appointment, o)}>
                            Изменить
                        </Button>
                    </Stack>
                ))}
            </Stack>
            <Stack direction="row" sx={{justifyContent: "space-between", alignItems: "baseline", mt: 1}}>
                <Typography variant="caption" color="text.secondary">Итого</Typography>
                <Typography variant="subtitle1" sx={{fontWeight: 700}}>
                    {formatPrice(appointment.price.value)}
                </Typography>
            </Stack>
            {ACTIONS[appointment.status].length > 0 && (
                <Stack direction="row" sx={{mt: 1.5, flexWrap: "wrap", gap: 1}}>
                    {(appointment.status === AppointmentStatus.Pending || appointment.status === AppointmentStatus.Confirmed) && (
                        <Button size="small" variant="outlined" disabled={busy} onClick={() => onReschedule(appointment)}>
                            Перенести
                        </Button>
                    )}
                    {ACTIONS[appointment.status].map(action => (
                        <Button
                            key={action.status}
                            size="small"
                            variant="outlined"
                            color={action.color}
                            disabled={busy}
                            onClick={() => onStatus(appointment.id, action.status)}
                        >
                            {action.label}
                        </Button>
                    ))}
                </Stack>
            )}
        </Paper>
    );
}

function Section({
    title,
    appointments,
    busyId,
    onStatus,
    onEdit,
    onReschedule,
}: {
    title: string;
    appointments: AdminAppointmentResponse[];
    busyId: string | null;
    onStatus: (id: string, status: AppointmentStatus) => void;
    onEdit: (appointment: AdminAppointmentResponse, offering: AdminAppointmentOffering) => void;
    onReschedule: (appointment: AdminAppointmentResponse) => void;
}) {
    return (
        <Stack spacing={1.5}>
            <Stack direction="row" spacing={1} sx={{alignItems: "baseline"}}>
                <Typography variant="overline" sx={{color: accentInk, letterSpacing: "0.14em"}}>
                    {title}
                </Typography>
                <Typography variant="caption" sx={{color: "text.secondary"}}>{appointments.length}</Typography>
            </Stack>
            {appointments.length === 0 && (
                <Typography variant="body2" sx={{color: "text.secondary"}}>Нет записей</Typography>
            )}
            {appointments.map(a => (
                <AppointmentCard
                    key={a.id}
                    appointment={a}
                    busy={busyId === a.id}
                    onStatus={onStatus}
                    onEdit={onEdit}
                    onReschedule={onReschedule}
                />
            ))}
        </Stack>
    );
}

export default function AppointmentsSection() {
    const [appointments, setAppointments] = useState<AdminAppointmentResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [busyId, setBusyId] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [filter, setFilter] = useState<Filter>("all");
    const [period, setPeriod] = useState<"all" | "day" | "week">("all");
    const [periodDate, setPeriodDate] = useState(() => formatRuDate(toIsoDate(new Date())));
    const [edit, setEdit] = useState<{appointment: AdminAppointmentResponse; offering: AdminAppointmentOffering} | null>(null);
    const [editPrice, setEditPrice] = useState("");
    const [editMinutes, setEditMinutes] = useState("");
    const [reschedule, setReschedule] = useState<AdminAppointmentResponse | null>(null);
    const [rescheduleDate, setRescheduleDate] = useState("");
    const [rescheduleTime, setRescheduleTime] = useState("10:00");
    const [rescheduleError, setRescheduleError] = useState<string | null>(null);

    async function reload() {
        setAppointments(await appointmentsApi.getAll());
    }

    useEffect(() => {
        let cancelled = false;
        appointmentsApi.getAll()
            .then(res => {
                if (!cancelled) setAppointments(res);
            })
            .catch(() => {
                if (!cancelled) setError("Не удалось загрузить записи");
            })
            .finally(() => {
                if (!cancelled) setLoading(false);
            });
        return () => {
            cancelled = true;
        };
    }, []);

    async function changeStatus(id: string, status: AppointmentStatus) {
        setBusyId(id);
        setError(null);
        try {
            await appointmentsApi.changeStatus({id, status});
            await reload();
        } catch (err) {
            setError(err instanceof Error ? err.message : "Не удалось изменить статус");
        } finally {
            setBusyId(null);
        }
    }

    function openEdit(appointment: AdminAppointmentResponse, offering: AdminAppointmentOffering) {
        setEdit({appointment, offering});
        setError(null);
        setEditPrice(String(offering.price.value));
        setEditMinutes(String(durationToMinutes(offering.duration)));
    }

    async function saveEdit() {
        if (!edit) return;
        const price = Number(editPrice);
        const minutes = Number(editMinutes);
        if (!Number.isFinite(price) || price <= 0 || !Number.isInteger(minutes) || minutes <= 0) {
            setError("Проверьте цену и длительность");
            return;
        }
        setBusyId(edit.appointment.id);
        setError(null);
        try {
            await appointmentsApi.changeOfferingDuration(
                edit.appointment.id,
                edit.offering.offeringId,
                minutesToDuration(minutes)
            );
            await appointmentsApi.changeOfferingPrice(edit.appointment.id, edit.offering.offeringId, {
                value: price,
                currency: edit.appointment.price.currency,
            });
            setEdit(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : "Не удалось сохранить изменения");
        } finally {
            setBusyId(null);
            reload().catch(() => {});
        }
    }

    function openReschedule(appointment: AdminAppointmentResponse) {
        setReschedule(appointment);
        setRescheduleDate(formatRuDate(appointment.date));
        setRescheduleTime(formatTimeOnly(appointment.interval.start));
        setRescheduleError(null);
    }

    async function saveReschedule() {
        if (!reschedule) return;
        const date = parseRuDate(rescheduleDate);
        const time = parseRuTime(rescheduleTime);
        if (!date || !time) {
            setRescheduleError("Проверьте формат даты и времени");
            return;
        }
        setBusyId(reschedule.id);
        setRescheduleError(null);
        try {
            await appointmentsApi.changeTime({id: reschedule.id, date, time: `${time}:00`});
            setReschedule(null);
            await reload();
        } catch (err) {
            setRescheduleError(err instanceof Error ? err.message : "Не удалось перенести запись");
        } finally {
            setBusyId(null);
        }
    }

    function inPeriod(appointment: AdminAppointmentResponse) {
        if (period === "all") return true;
        const iso = parseRuDate(periodDate);
        if (!iso) return true;
        if (period === "day") return appointment.date === iso;
        const base = new Date(`${iso}T00:00:00`);
        const mondayOffset = (base.getDay() + 6) % 7;
        const monday = new Date(base);
        monday.setDate(base.getDate() - mondayOffset);
        const sunday = new Date(monday);
        sunday.setDate(monday.getDate() + 6);
        const value = new Date(`${appointment.date}T00:00:00`);
        return value >= monday && value <= sunday;
    }

    if (loading) {
        return <Stack sx={{alignItems: "center", py: 2}}><CircularProgress size={22}/></Stack>;
    }

    const now = new Date();
    const filtered = appointments.filter(a => (filter === "all" || a.status === filter) && inPeriod(a));
    const upcoming = filtered.filter(a => startOf(a) > now).sort((a, b) => +startOf(a) - +startOf(b));
    const past = filtered.filter(a => startOf(a) <= now).sort((a, b) => +startOf(b) - +startOf(a));

    return (
        <Stack spacing={3}>
            {error && <Alert severity="error">{error}</Alert>}
            <Stack direction={{xs: "column", sm: "row"}} spacing={2} sx={{alignItems: {sm: "center"}, flexWrap: "wrap"}}>
                <Select
                    size="small"
                    value={period}
                    onChange={e => setPeriod(e.target.value as "all" | "day" | "week")}
                    sx={{minWidth: 160, borderRadius: 2}}
                >
                    <MenuItem value="all">Весь период</MenuItem>
                    <MenuItem value="day">День</MenuItem>
                    <MenuItem value="week">Неделя</MenuItem>
                </Select>
                {period !== "all" && (
                    <DateField label="Дата" value={periodDate} onChange={setPeriodDate}/>
                )}
                <Select
                    size="small"
                    value={filter}
                    onChange={e => setFilter(e.target.value as Filter)}
                    sx={{minWidth: 200, borderRadius: 2}}
                >
                    <MenuItem value="all">Все статусы</MenuItem>
                    {STATUS_ORDER.map(status => (
                        <MenuItem key={status} value={status}>{STATUS_META[status].label}</MenuItem>
                    ))}
                </Select>
            </Stack>
            <Section
                title="Предстоящие"
                appointments={upcoming}
                busyId={busyId}
                onStatus={changeStatus}
                onEdit={openEdit}
                onReschedule={openReschedule}
            />
            <Section
                title="Прошедшие"
                appointments={past}
                busyId={busyId}
                onStatus={changeStatus}
                onEdit={openEdit}
                onReschedule={openReschedule}
            />
            <Dialog open={edit !== null} onClose={() => setEdit(null)} fullWidth maxWidth="xs">
                <DialogTitle>Изменить услугу</DialogTitle>
                <DialogContent>
                    <Stack spacing={2} sx={{pt: 1}}>
                        <Typography variant="body2" color="text.secondary">
                            {edit?.offering.title}
                        </Typography>
                        {error && <Alert severity="error">{error}</Alert>}
                        <TextField
                            label="Цена, ₽"
                            type="number"
                            value={editPrice}
                            onChange={e => setEditPrice(e.target.value)}
                            fullWidth
                        />
                        <TextField
                            label="Длительность, мин"
                            type="number"
                            value={editMinutes}
                            onChange={e => setEditMinutes(e.target.value)}
                            fullWidth
                        />
                    </Stack>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setEdit(null)}>Отмена</Button>
                    <Button variant="contained" disabled={busyId !== null} onClick={saveEdit}>
                        Сохранить
                    </Button>
                </DialogActions>
            </Dialog>
            <Dialog open={reschedule !== null} onClose={() => setReschedule(null)} fullWidth maxWidth="xs">
                <DialogTitle>Перенести запись</DialogTitle>
                <DialogContent>
                    <Stack spacing={2} sx={{pt: 1}}>
                        {reschedule && (
                            <Typography variant="body2" color="text.secondary">
                                {reschedule.userName} · {formatDayLabel(reschedule.date)}
                            </Typography>
                        )}
                        {rescheduleError && <Alert severity="error">{rescheduleError}</Alert>}
                        <DateField label="Новая дата" value={rescheduleDate} onChange={setRescheduleDate}/>
                        <TimeField label="Новое время" value={rescheduleTime} onChange={setRescheduleTime}/>
                    </Stack>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setReschedule(null)}>Отмена</Button>
                    <Button variant="contained" disabled={busyId !== null} onClick={saveReschedule}>
                        Перенести
                    </Button>
                </DialogActions>
            </Dialog>
        </Stack>
    );
}
