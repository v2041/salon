"use client";

import {Alert, Button, Chip, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, Paper, Select, Stack, TextField, Typography} from "@mui/material";
import {useEffect, useState} from "react";
import {appointmentsApi} from "@/lib/api";
import {durationToMinutes, formatDayLabel, formatDurationValue, formatPrice, formatTimeOnly} from "@/lib/format";
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

function toDurationString(minutes: number) {
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return `${String(h).padStart(2, "0")}:${String(m).padStart(2, "0")}:00`;
}

function startOf(a: AdminAppointmentResponse) {
    return new Date(`${a.date}T${a.interval.start}`);
}

function AppointmentCard({
    appointment,
    busy,
    onStatus,
    onEdit,
}: {
    appointment: AdminAppointmentResponse;
    busy: boolean;
    onStatus: (id: string, status: AppointmentStatus) => void;
    onEdit: (appointment: AdminAppointmentResponse, offering: AdminAppointmentOffering) => void;
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
}: {
    title: string;
    appointments: AdminAppointmentResponse[];
    busyId: string | null;
    onStatus: (id: string, status: AppointmentStatus) => void;
    onEdit: (appointment: AdminAppointmentResponse, offering: AdminAppointmentOffering) => void;
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
    const [edit, setEdit] = useState<{appointment: AdminAppointmentResponse; offering: AdminAppointmentOffering} | null>(null);
    const [editPrice, setEditPrice] = useState("");
    const [editMinutes, setEditMinutes] = useState("");

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
            await Promise.all([
                appointmentsApi.changeOfferingPrice(edit.appointment.id, edit.offering.offeringId, {
                    value: price,
                    currency: edit.appointment.price.currency,
                }),
                appointmentsApi.changeOfferingDuration(
                    edit.appointment.id,
                    edit.offering.offeringId,
                    toDurationString(minutes)
                ),
            ]);
            setEdit(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : "Не удалось сохранить изменения");
        } finally {
            setBusyId(null);
            reload().catch(() => {});
        }
    }

    if (loading) {
        return <Stack sx={{alignItems: "center", py: 2}}><CircularProgress size={22}/></Stack>;
    }

    const now = new Date();
    const filtered = filter === "all" ? appointments : appointments.filter(a => a.status === filter);
    const upcoming = filtered.filter(a => startOf(a) > now).sort((a, b) => +startOf(a) - +startOf(b));
    const past = filtered.filter(a => startOf(a) <= now).sort((a, b) => +startOf(b) - +startOf(a));

    return (
        <Stack spacing={3}>
            {error && <Alert severity="error">{error}</Alert>}
            <Select
                size="small"
                value={filter}
                onChange={e => setFilter(e.target.value as Filter)}
                sx={{alignSelf: "flex-start", minWidth: 200, borderRadius: 2}}
            >
                <MenuItem value="all">Все статусы</MenuItem>
                {STATUS_ORDER.map(status => (
                    <MenuItem key={status} value={status}>{STATUS_META[status].label}</MenuItem>
                ))}
            </Select>
            <Section
                title="Предстоящие"
                appointments={upcoming}
                busyId={busyId}
                onStatus={changeStatus}
                onEdit={openEdit}
            />
            <Section
                title="Прошедшие"
                appointments={past}
                busyId={busyId}
                onStatus={changeStatus}
                onEdit={openEdit}
            />
            <Dialog open={edit !== null} onClose={() => setEdit(null)} fullWidth maxWidth="xs">
                <DialogTitle>Изменить услугу</DialogTitle>
                <DialogContent>
                    <Stack spacing={2} sx={{pt: 1}}>
                        <Typography variant="body2" color="text.secondary">
                            {edit?.offering.title}
                        </Typography>
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
        </Stack>
    );
}
