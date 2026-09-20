import {AccessTime as AccessTimeIcon} from "@mui/icons-material";
import {Box, Divider, List, ListItem, Paper, Stack, Typography} from "@mui/material";
import {formatDayLabel, formatDuration, formatPrice, formatTime} from "@/lib/format";

export enum AppointmentStatus {
    Pending = 0,
    Confirmed = 1,
    Completed = 2,
    Canceled = 3,
    Missed = 4,
    Rejected = 5,
}

type Service = {
    id: number;
    name: string;
    price: number;
};

export type Appointment = {
    id: number;
    start: string;
    end: string;
    status: AppointmentStatus;
    services: Service[];
};

const APPOINTMENTS: Appointment[] = [
    {
        id: 1,
        start: "2026-09-15T15:00:00",
        end: "2026-09-15T16:30:00",
        status: AppointmentStatus.Confirmed,
        services: [
            {id: 1, name: "Маникюр", price: 2000},
            {id: 2, name: "Стрижка", price: 600},
        ],
    },
    {
        id: 2,
        start: "2026-09-16T12:00:00",
        end: "2026-09-16T13:30:00",
        status: AppointmentStatus.Pending,
        services: [
            {id: 3, name: "Окрашивание", price: 4500},
            {id: 4, name: "Укладка", price: 1200},
        ],
    },
    {
        id: 3,
        start: "2026-09-10T18:00:00",
        end: "2026-09-10T18:45:00",
        status: AppointmentStatus.Completed,
        services: [
            {id: 5, name: "Стрижка", price: 600},
        ],
    },
    {
        id: 4,
        start: "2026-09-08T14:00:00",
        end: "2026-09-08T15:30:00",
        status: AppointmentStatus.Missed,
        services: [
            {id: 6, name: "Маникюр", price: 2000},
        ],
    },
    {
        id: 5,
        start: "2026-09-06T12:00:00",
        end: "2026-09-06T13:00:00",
        status: AppointmentStatus.Canceled,
        services: [
            {id: 7, name: "Окрашивание", price: 4500},
        ],
    },
    {
        id: 6,
        start: "2026-09-03T10:00:00",
        end: "2026-09-03T10:45:00",
        status: AppointmentStatus.Rejected,
        services: [
            {id: 8, name: "Стрижка", price: 600},
        ],
    },
];

const NOW = new Date();

type StatusMeta = {label: string; color: string};

const STATUS_META: Record<AppointmentStatus, StatusMeta> = {
    [AppointmentStatus.Pending]: {label: "В рассмотрении", color: "warning.main"},
    [AppointmentStatus.Confirmed]: {label: "Подтверждена", color: "success.main"},
    [AppointmentStatus.Completed]: {label: "Завершена", color: "text.secondary"},
    [AppointmentStatus.Canceled]: {label: "Отменена", color: "error.main"},
    [AppointmentStatus.Missed]: {label: "Пропущена", color: "error.main"},
    [AppointmentStatus.Rejected]: {label: "Отклонена", color: "error.main"},
};

export function AppointmentCard({appointment, muted}: {appointment: Appointment; muted: boolean}) {
    const total = appointment.services.reduce((sum, s) => sum + s.price, 0);
    const status = STATUS_META[appointment.status];
    return (
        <Paper variant="outlined" elevation={1} sx={{borderRadius: 2, p: 2, borderColor: "text.disabled"}}>
            <Stack direction="row" sx={{justifyContent: "space-between", alignItems: "baseline"}}>
                <Typography variant="subtitle1" sx={{fontWeight: 600}} color={muted ? "text.secondary" : "text.primary"}>
                    {formatDayLabel(appointment.start)}
                </Typography>
                <Typography variant="body2" sx={{fontWeight: 600}} color={muted ? "text.secondary" : "primary"}>
                    {formatPrice(total)}
                </Typography>
            </Stack>
            <Stack direction="row" sx={{alignItems: "center", justifyContent: "space-between", mt: 0.5}}>
                <Stack direction="row" spacing={1} sx={{alignItems: "center"}}>
                    <AccessTimeIcon sx={{fontSize: 16}} color={muted ? "disabled" : "action"}/>
                    <Typography variant="body2" color="text.secondary">
                        {formatTime(appointment.start)}–{formatTime(appointment.end)} ·{" "}
                        {formatDuration(appointment.start, appointment.end)}
                    </Typography>
                </Stack>
                <Typography variant="body2" sx={{fontWeight: 500, color: status.color}}>
                    {status.label}
                </Typography>
            </Stack>
            <Divider sx={{my: 1.5}}/>
            <List disablePadding>
                {appointment.services.map(s => (
                    <ListItem key={s.id} disableGutters sx={{py: 0.5}}>
                        <Box sx={{display: "flex", justifyContent: "space-between", width: "100%"}}>
                            <Typography variant="body2" color={muted ? "text.secondary" : "text.primary"}>
                                {s.name}
                            </Typography>
                            <Typography variant="body2" color={muted ? "text.secondary" : "text.primary"}>
                                {formatPrice(s.price)}
                            </Typography>
                        </Box>
                    </ListItem>
                ))}
            </List>
        </Paper>
    );
}

function Section({title, appointments, muted}: {title: string; appointments: Appointment[]; muted: boolean}) {
    if (appointments.length === 0) {
        return (
            <Stack spacing={1}>
                <Typography variant="h6">{title}</Typography>
                <Typography color="text.secondary">Нет записей</Typography>
            </Stack>
        );
    }
    return (
        <Stack spacing={3}>
            <Typography variant="h6">{title}</Typography>
            {appointments.map(a => <AppointmentCard key={a.id} appointment={a} muted={muted}/>)}
        </Stack>
    );
}

export default function MyAppointmentsList() {
    const upcoming = APPOINTMENTS
        .filter(a => new Date(a.start) > NOW)
        .sort((a, b) => +new Date(a.start) - +new Date(b.start));
    const past = APPOINTMENTS
        .filter(a => new Date(a.end) <= NOW)
        .sort((a, b) => +new Date(b.start) - +new Date(a.start));

    return (
        <Stack spacing={4}>
            <Section title="Предстоящие" appointments={upcoming} muted={false}/>
            <Section title="Прошлые" appointments={past} muted={true}/>
        </Stack>
    );
}
