"use client";

import {AccessTime as AccessTimeIcon} from "@mui/icons-material";
import {Alert, Box, Chip, CircularProgress, Divider, Paper, Stack, Typography} from "@mui/material";
import {useEffect, useState} from "react";
import {appointmentsApi} from "@/lib/api";
import {formatDayLabel, formatDurationValue, formatPrice, formatTimeOnly, timeToMinutes} from "@/lib/format";
import {cardShadow} from "@/lib/theme";
import {AppointmentStatus} from "@/lib/types";
import type {UserAppointmentResponse} from "@/lib/types";

const STATUS_META: Record<AppointmentStatus, {label: string; color: "warning" | "success" | "default" | "error"}> = {
    [AppointmentStatus.Pending]: {label: "Ожидает", color: "warning"},
    [AppointmentStatus.Confirmed]: {label: "Подтверждена", color: "success"},
    [AppointmentStatus.Completed]: {label: "Завершена", color: "default"},
    [AppointmentStatus.Canceled]: {label: "Отменена", color: "error"},
    [AppointmentStatus.Missed]: {label: "Пропущена", color: "error"},
    [AppointmentStatus.Rejected]: {label: "Отклонена", color: "error"},
};

const cardSx = {
    p: 2,
    borderRadius: "20px",
    border: 1,
    borderColor: "divider",
    bgcolor: "background.paper",
    boxShadow: cardShadow,
};

function startOf(a: UserAppointmentResponse) {
    return new Date(`${a.date}T${a.interval.start}`);
}

function AppointmentCard({appointment, muted}: {appointment: UserAppointmentResponse; muted: boolean}) {
    const status = STATUS_META[appointment.status];
    const duration = timeToMinutes(appointment.interval.end) - timeToMinutes(appointment.interval.start);
    return (
        <Paper sx={{...cardSx, opacity: muted ? 0.75 : 1}}>
            <Stack direction="row" sx={{justifyContent: "space-between", alignItems: "center", mb: 0.5}}>
                <Typography variant="subtitle1" sx={{fontWeight: 650}}>
                    {formatDayLabel(appointment.date)}
                </Typography>
                <Chip label={status.label} size="small" color={status.color} variant="outlined"/>
            </Stack>
            <Stack direction="row" spacing={1} sx={{alignItems: "center"}}>
                <AccessTimeIcon sx={{fontSize: 15, color: "text.secondary"}}/>
                <Typography variant="body2" color="text.secondary">
                    {formatTimeOnly(appointment.interval.start)}–{formatTimeOnly(appointment.interval.end)}
                    {" · "}
                    {formatDurationValue(duration)}
                </Typography>
            </Stack>
            <Divider sx={{my: 1.5}}/>
            <Stack spacing={0.5}>
                {appointment.offerings.map((o, i) => (
                    <Stack key={i} direction="row" sx={{justifyContent: "space-between", gap: 2}}>
                        <Typography variant="body2" color="text.secondary">{o.title}</Typography>
                        <Typography variant="body2" color="text.secondary" sx={{whiteSpace: "nowrap"}}>
                            {formatPrice(o.price.value)}
                        </Typography>
                    </Stack>
                ))}
            </Stack>
            <Stack direction="row" sx={{justifyContent: "space-between", alignItems: "baseline", mt: 1.5}}>
                <Typography variant="caption" color="text.secondary">Итого</Typography>
                <Typography variant="subtitle1" sx={{fontWeight: 700}}>{formatPrice(appointment.price.value)}</Typography>
            </Stack>
        </Paper>
    );
}

function Section({
    title,
    appointments,
    muted,
}: {
    title: string;
    appointments: UserAppointmentResponse[];
    muted: boolean;
}) {
    return (
        <Stack spacing={1.5}>
            <Stack direction="row" spacing={1} sx={{alignItems: "baseline"}}>
                <Typography variant="overline" sx={{color: "text.secondary", letterSpacing: "0.14em"}}>
                    {title}
                </Typography>
                <Typography variant="caption" color="text.secondary">{appointments.length}</Typography>
            </Stack>
            {appointments.length === 0 && (
                <Typography variant="body2" color="text.secondary">Нет записей</Typography>
            )}
            {appointments.map(a => (
                <AppointmentCard key={a.id} appointment={a} muted={muted}/>
            ))}
        </Stack>
    );
}

export default function MyAppointmentsList() {
    const [appointments, setAppointments] = useState<UserAppointmentResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        appointmentsApi.getByUser()
            .then(res => {
                if (!cancelled) {
                    setAppointments(res);
                    setError(null);
                }
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

    if (loading) {
        return <Box sx={{display: "flex", justifyContent: "center", py: 4}}><CircularProgress size={24}/></Box>;
    }

    if (error) {
        return <Alert severity="error">{error}</Alert>;
    }

    const now = new Date();
    const upcoming = appointments
        .filter(a => startOf(a) > now)
        .sort((a, b) => +startOf(a) - +startOf(b));
    const past = appointments
        .filter(a => startOf(a) <= now)
        .sort((a, b) => +startOf(b) - +startOf(a));

    if (appointments.length === 0) {
        return <Alert severity="info">Записей пока нет</Alert>;
    }

    return (
        <Stack spacing={3.5}>
            <Section title="Предстоящие" appointments={upcoming} muted={false}/>
            <Section title="Прошлые" appointments={past} muted={true}/>
        </Stack>
    );
}
