"use client";

import {Alert, Button, Chip, CircularProgress, Dialog, DialogActions, DialogContent, DialogTitle, MenuItem, Paper, Select, Stack, TextField, Typography} from "@mui/material";
import {useEffect, useRef, useState} from "react";
import {appointmentsApi} from "@/lib/api";
import {durationToMinutes, formatDayLabel, formatDurationValue, formatPrice, formatTimeOnly, minutesToDuration} from "@/lib/format";
import {accentInk, cardSx} from "@/lib/theme";
import {AppointmentStatus} from "@/lib/types";
import type {AdminAppointmentOffering, AdminAppointmentResponse, AppointmentScope} from "@/lib/types";

const PAGE_SIZE = 20;

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

type SectionData = {
    items: AdminAppointmentResponse[];
    page: number;
    totalCount: number;
};

const EMPTY_SECTION: SectionData = {items: [], page: 0, totalCount: 0};

type CardProps = {
    appointment: AdminAppointmentResponse;
    busy: boolean;
    onStatus: (id: string, status: AppointmentStatus) => void;
    onEdit: (appointment: AdminAppointmentResponse, offering: AdminAppointmentOffering) => void;
};

function AppointmentCard({
    appointment,
    busy,
    onStatus,
    onEdit,
}: CardProps) {
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

type SectionProps = {
    title: string;
    appointments: AdminAppointmentResponse[];
    busyId: string | null;
    totalCount: number;
    loadMoreDisabled: boolean;
    onStatus: (id: string, status: AppointmentStatus) => void;
    onEdit: (appointment: AdminAppointmentResponse, offering: AdminAppointmentOffering) => void;
    onLoadMore: () => void;
};

function Section({
    title,
    appointments,
    busyId,
    totalCount,
    loadMoreDisabled,
    onStatus,
    onEdit,
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
                <AppointmentCard
                    key={a.id}
                    appointment={a}
                    busy={busyId === a.id}
                    onStatus={onStatus}
                    onEdit={onEdit}
                />
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

export default function AppointmentsSection() {
    const [upcoming, setUpcoming] = useState<SectionData>(EMPTY_SECTION);
    const [past, setPast] = useState<SectionData>(EMPTY_SECTION);
    const [loading, setLoading] = useState(true);
    const [pending, setPending] = useState(false);
    const [loadingMore, setLoadingMore] = useState<AppointmentScope | null>(null);
    const [busyId, setBusyId] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [filter, setFilter] = useState<Filter>("all");
    const [edit, setEdit] = useState<{appointment: AdminAppointmentResponse; offering: AdminAppointmentOffering} | null>(null);
    const [editPrice, setEditPrice] = useState("");
    const [editMinutes, setEditMinutes] = useState("");

    const generation = useRef(0);

    async function loadSection(scope: AppointmentScope, page: number, filterValue: Filter): Promise<SectionData> {
        const res = await appointmentsApi.getAll({
            scope,
            page,
            pageSize: PAGE_SIZE,
            status: filterValue === "all" ? undefined : filterValue,
        });
        return {items: res.items, page: res.page, totalCount: res.totalCount};
    }

    async function refresh(filterValue: Filter) {
        const request = ++generation.current;
        setPending(true);
        setLoadingMore(null);
        setError(null);
        try {
            const [upcomingPage, pastPage] = await Promise.all([
                loadSection("upcoming", 1, filterValue),
                loadSection("past", 1, filterValue),
            ]);
            if (request !== generation.current) return;
            setUpcoming(upcomingPage);
            setPast(pastPage);
        } catch {
            if (request !== generation.current) return;
            setError("Не удалось загрузить записи");
        } finally {
            if (request === generation.current) setPending(false);
        }
    }

    useEffect(() => {
        let cancelled = false;
        Promise.all([
            loadSection("upcoming", 1, "all"),
            loadSection("past", 1, "all"),
        ])
            .then(([upcomingPage, pastPage]) => {
                if (cancelled) return;
                setUpcoming(upcomingPage);
                setPast(pastPage);
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
        const request = generation.current;
        const current = scope === "upcoming" ? upcoming : past;
        setLoadingMore(scope);
        setError(null);
        try {
            const res = await appointmentsApi.getAll({
                scope,
                page: current.page + 1,
                pageSize: PAGE_SIZE,
                status: filter === "all" ? undefined : filter,
            });
            if (request !== generation.current) return;
            const apply = (prev: SectionData): SectionData => ({
                items: [...prev.items, ...res.items],
                page: res.page,
                totalCount: res.totalCount,
            });
            if (scope === "upcoming") setUpcoming(apply);
            else setPast(apply);
        } catch {
            if (request !== generation.current) return;
            setError("Не удалось загрузить записи");
        } finally {
            if (request === generation.current) setLoadingMore(prev => (prev === scope ? null : prev));
        }
    }

    function changeFilter(value: Filter) {
        setFilter(value);
        refresh(value);
    }

    async function changeStatus(id: string, status: AppointmentStatus) {
        setBusyId(id);
        setError(null);
        try {
            await appointmentsApi.changeStatus({id, status});
            await refresh(filter);
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
            await refresh(filter);
        }
    }

    if (loading) {
        return <Stack sx={{alignItems: "center", py: 2}}><CircularProgress size={22}/></Stack>;
    }

    return (
        <Stack spacing={3}>
            {error && <Alert severity="error">{error}</Alert>}
            <Select
                size="small"
                value={filter}
                disabled={pending}
                onChange={e => changeFilter(e.target.value as Filter)}
                sx={{alignSelf: "flex-start", minWidth: 200, borderRadius: 2}}
            >
                <MenuItem value="all">Все статусы</MenuItem>
                {STATUS_ORDER.map(status => (
                    <MenuItem key={status} value={status}>{STATUS_META[status].label}</MenuItem>
                ))}
            </Select>
            <Section
                title="Предстоящие"
                appointments={upcoming.items}
                busyId={busyId}
                totalCount={upcoming.totalCount}
                loadMoreDisabled={pending || loadingMore === "upcoming"}
                onStatus={changeStatus}
                onEdit={openEdit}
                onLoadMore={() => loadMore("upcoming")}
            />
            <Section
                title="Прошедшие"
                appointments={past.items}
                busyId={busyId}
                totalCount={past.totalCount}
                loadMoreDisabled={pending || loadingMore === "past"}
                onStatus={changeStatus}
                onEdit={openEdit}
                onLoadMore={() => loadMore("past")}
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
        </Stack>
    );
}
