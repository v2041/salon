"use client";

import {AccessTime as AccessTimeIcon} from "@mui/icons-material";
import {Alert, Box, Button, Chip, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle, Divider, Paper, Stack, Typography} from "@mui/material";
import {useEffect, useState} from "react";
import {appointmentsApi} from "@/lib/api";
import {formatDayLabel, formatDurationValue, formatPrice, formatTimeOnly, timeToMinutes} from "@/lib/format";
import {accentInk, cardSx} from "@/lib/theme";
import {AppointmentStatus} from "@/lib/types";
import type {AppointmentScope, UserAppointmentResponse} from "@/lib/types";

const PAGE_SIZE = 20;

const STATUS_META: Record<AppointmentStatus, {label: string; color: "warning" | "success" | "default" | "error"}> = {
    [AppointmentStatus.Pending]: {label: "Ожидает", color: "warning"},
    [AppointmentStatus.Confirmed]: {label: "Подтверждена", color: "success"},
    [AppointmentStatus.Completed]: {label: "Завершена", color: "default"},
    [AppointmentStatus.Canceled]: {label: "Отменена", color: "error"},
    [AppointmentStatus.Missed]: {label: "Пропущена", color: "error"},
    [AppointmentStatus.Rejected]: {label: "Отклонена", color: "error"},
};

function startOf(a: UserAppointmentResponse) {
    return new Date(`${a.date}T${a.interval.start}`);
}

type CardProps = {
    appointment: UserAppointmentResponse;
    muted: boolean;
    onCancel: (appointment: UserAppointmentResponse) => void;
};

function AppointmentCard({
    appointment,
    muted,
    onCancel,
}: CardProps) {
    const status = STATUS_META[appointment.status];
    const duration = timeToMinutes(appointment.interval.end) - timeToMinutes(appointment.interval.start);
    const now = new Date();
    const startsAt = startOf(appointment).getTime();
    const cancellable =
        startsAt > +now &&
        (appointment.status === AppointmentStatus.Pending || appointment.status === AppointmentStatus.Confirmed);
    const cancelAllowed = appointment.status === AppointmentStatus.Pending || startsAt - +now > 2 * 60 * 60 * 1000;
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
            {cancellable && (
                <Stack spacing={0.5} sx={{mt: 1.5, alignItems: "flex-end"}}>
                    <Button
                        size="small"
                        variant="outlined"
                        color="error"
                        disabled={!cancelAllowed}
                        onClick={() => onCancel(appointment)}
                    >
                        Отменить
                    </Button>
                    {!cancelAllowed && (
                        <Typography variant="caption" color="text.secondary">
                            Отмена доступна не позднее чем за 2 часа до начала
                        </Typography>
                    )}
                </Stack>
            )}
        </Paper>
    );
}

type SectionData = {
    items: UserAppointmentResponse[];
    page: number;
    totalCount: number;
};

const EMPTY_SECTION: SectionData = {items: [], page: 0, totalCount: 0};

type SectionProps = {
    title: string;
    appointments: UserAppointmentResponse[];
    muted: boolean;
    totalCount: number;
    loadMoreDisabled: boolean;
    onCancel: (appointment: UserAppointmentResponse) => void;
    onLoadMore: () => void;
};

function Section({
    title,
    appointments,
    muted,
    totalCount,
    loadMoreDisabled,
    onCancel,
    onLoadMore,
}: SectionProps) {
    return (
        <Stack spacing={1.5}>
            <Stack direction="row" spacing={1} sx={{alignItems: "baseline"}}>
                <Typography variant="overline" sx={{color: accentInk, letterSpacing: "0.14em"}}>
                    {title}
                </Typography>
                {totalCount > 0 && (
                    <Typography variant="caption" sx={{color: "text.secondary"}}>
                        Показано {appointments.length} из {totalCount}
                    </Typography>
                )}
            </Stack>
            {appointments.length === 0 && (
                <Typography variant="body2" sx={{color: "text.secondary"}}>Нет записей</Typography>
            )}
            {appointments.map(a => (
                <AppointmentCard key={a.id} appointment={a} muted={muted} onCancel={onCancel}/>
            ))}
            {appointments.length < totalCount && (
                <Button
                    size="small"
                    variant="outlined"
                    disabled={loadMoreDisabled}
                    onClick={onLoadMore}
                    sx={{alignSelf: "center", minWidth: 160}}
                >
                    Показать ещё
                </Button>
            )}
        </Stack>
    );
}

export default function MyAppointmentsList() {
    const [upcoming, setUpcoming] = useState<SectionData>(EMPTY_SECTION);
    const [past, setPast] = useState<SectionData>(EMPTY_SECTION);
    const [loading, setLoading] = useState(true);
    const [loadingMore, setLoadingMore] = useState<AppointmentScope | null>(null);
    const [cancelTarget, setCancelTarget] = useState<UserAppointmentResponse | null>(null);
    const [canceling, setCanceling] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;
        Promise.all([
            appointmentsApi.getByUser({scope: "upcoming", page: 1, pageSize: PAGE_SIZE}),
            appointmentsApi.getByUser({scope: "past", page: 1, pageSize: PAGE_SIZE}),
        ])
            .then(([upcomingPage, pastPage]) => {
                if (cancelled) return;
                setUpcoming({items: upcomingPage.items, page: upcomingPage.page, totalCount: upcomingPage.totalCount});
                setPast({items: pastPage.items, page: pastPage.page, totalCount: pastPage.totalCount});
                setError(null);
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

    async function loadMore(scope: AppointmentScope) {
        const current = scope === "upcoming" ? upcoming : past;
        setLoadingMore(scope);
        setError(null);
        try {
            const res = await appointmentsApi.getByUser({scope, page: current.page + 1, pageSize: PAGE_SIZE});
            const apply = (prev: SectionData): SectionData => ({
                items: [...prev.items, ...res.items],
                page: res.page,
                totalCount: res.totalCount,
            });
            if (scope === "upcoming") setUpcoming(apply);
            else setPast(apply);
        } catch {
            setError("Не удалось загрузить записи");
        } finally {
            setLoadingMore(prev => (prev === scope ? null : prev));
        }
    }

    function requestCancel(appointment: UserAppointmentResponse) {
        setError(null);
        setCancelTarget(appointment);
    }

    function closeCancel() {
        if (canceling) return;
        setCancelTarget(null);
        setError(null);
    }

    async function confirmCancel() {
        if (!cancelTarget) return;
        const id = cancelTarget.id;
        setCanceling(true);
        setError(null);
        try {
            await appointmentsApi.cancel(id);
            const apply = (prev: SectionData): SectionData => ({
                ...prev,
                items: prev.items.map(a => (a.id === id ? {...a, status: AppointmentStatus.Canceled} : a)),
            });
            setUpcoming(apply);
            setPast(apply);
            setCancelTarget(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : "Не удалось отменить запись");
        } finally {
            setCanceling(false);
        }
    }

    if (loading) {
        return <Box sx={{display: "flex", justifyContent: "center", py: 4}}><CircularProgress size={24} sx={{color: accentInk}}/></Box>;
    }

    const totalLoaded = upcoming.items.length + past.items.length;

    if (error && totalLoaded === 0) {
        return <Alert severity="error">{error}</Alert>;
    }

    if (upcoming.totalCount === 0 && past.totalCount === 0) {
        return <Alert severity="info">Записей пока нет</Alert>;
    }

    return (
        <Stack spacing={3.5}>
            {error && cancelTarget === null && <Alert severity="error">{error}</Alert>}
            <Section
                title="Предстоящие"
                appointments={upcoming.items}
                muted={false}
                totalCount={upcoming.totalCount}
                loadMoreDisabled={loadingMore === "upcoming"}
                onCancel={requestCancel}
                onLoadMore={() => loadMore("upcoming")}
            />
            <Section
                title="Прошедшие"
                appointments={past.items}
                muted={true}
                totalCount={past.totalCount}
                loadMoreDisabled={loadingMore === "past"}
                onCancel={requestCancel}
                onLoadMore={() => loadMore("past")}
            />
            <Dialog open={cancelTarget !== null} onClose={closeCancel} fullWidth maxWidth="xs">
                <DialogTitle>Отменить запись?</DialogTitle>
                <DialogContent>
                    <Stack spacing={1.5} sx={{pt: 0.5}}>
                        {cancelTarget && (
                            <Typography variant="body2" color="text.secondary">
                                {formatDayLabel(cancelTarget.date)} ·{" "}
                                {formatTimeOnly(cancelTarget.interval.start)}–{formatTimeOnly(cancelTarget.interval.end)}
                            </Typography>
                        )}
                        <Typography variant="body2">После отмены запись нельзя восстановить.</Typography>
                        {error && <Alert severity="error">{error}</Alert>}
                    </Stack>
                </DialogContent>
                <DialogActions>
                    <Button onClick={closeCancel} disabled={canceling}>Не отменять</Button>
                    <Button variant="contained" color="error" disabled={canceling} onClick={confirmCancel}>
                        Отменить запись
                    </Button>
                </DialogActions>
            </Dialog>
        </Stack>
    );
}
