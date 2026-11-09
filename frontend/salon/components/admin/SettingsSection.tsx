"use client";

import {
    Alert,
    Button,
    Checkbox,
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    FormControlLabel,
    Paper,
    Stack,
    Switch,
    TextField,
    Typography,
} from "@mui/material";
import {useEffect, useState} from "react";
import TimeField from "@/components/admin/TimeField";
import {settingsApi} from "@/lib/api";
import {formatTimeOnly, parseRuTime, plural, timeToMinutes} from "@/lib/format";
import {cardSx} from "@/lib/theme";
import {DayOfWeek, type ChangeSettingsRequest, type SettingsResponse} from "@/lib/types";

const WEEKDAYS: {value: DayOfWeek; label: string}[] = [
    {value: DayOfWeek.Monday, label: "Пн"},
    {value: DayOfWeek.Tuesday, label: "Вт"},
    {value: DayOfWeek.Wednesday, label: "Ср"},
    {value: DayOfWeek.Thursday, label: "Чт"},
    {value: DayOfWeek.Friday, label: "Пт"},
    {value: DayOfWeek.Saturday, label: "Сб"},
    {value: DayOfWeek.Sunday, label: "Вс"},
];

const MIN_HORIZON = 1;
const MAX_HORIZON = 60;
const MIN_WORK_MINUTES = 120;

type Form = {
    address: string;
    phone: string;
    workStart: string;
    workEnd: string;
    breakOn: boolean;
    breakStart: string;
    breakEnd: string;
    generationEnabled: boolean;
    horizonDays: string;
    weekends: DayOfWeek[];
};

function fromSettings(settings: SettingsResponse): Form {
    return {
        address: settings.address,
        phone: settings.phone,
        workStart: formatTimeOnly(settings.workInterval.start),
        workEnd: formatTimeOnly(settings.workInterval.end),
        breakOn: settings.breakInterval !== null,
        breakStart: settings.breakInterval ? formatTimeOnly(settings.breakInterval.start) : "13:00",
        breakEnd: settings.breakInterval ? formatTimeOnly(settings.breakInterval.end) : "14:00",
        generationEnabled: settings.generationEnabled,
        horizonDays: String(settings.horizonDays),
        weekends: [...settings.weekendDays],
    };
}

function validate(form: Form): string | null {
    if (!form.address.trim()) return "Введите адрес.";
    if (form.address.trim().length > 200) return "Адрес — не больше 200 символов.";
    if (!/^\+7\d{10}$/.test(form.phone.trim())) return "Телефон должен быть в формате +7XXXXXXXXXX.";
    const workStart = parseRuTime(form.workStart);
    const workEnd = parseRuTime(form.workEnd);
    if (!workStart || !workEnd) return "Проверьте время работы — чч:мм.";
    if (timeToMinutes(workEnd) - timeToMinutes(workStart) < MIN_WORK_MINUTES) {
        return "Рабочий день — не меньше 2 часов.";
    }
    if (form.breakOn) {
        const breakStart = parseRuTime(form.breakStart);
        const breakEnd = parseRuTime(form.breakEnd);
        if (!breakStart || !breakEnd) return "Проверьте время перерыва — чч:мм.";
        if (timeToMinutes(breakEnd) <= timeToMinutes(breakStart)) return "Конец перерыва должен быть позже начала.";
        if (timeToMinutes(breakStart) <= timeToMinutes(workStart) || timeToMinutes(breakEnd) >= timeToMinutes(workEnd)) {
            return "Перерыв должен быть внутри рабочего времени.";
        }
    }
    const horizon = Number(form.horizonDays);
    if (!Number.isInteger(horizon) || horizon < MIN_HORIZON || horizon > MAX_HORIZON) {
        return `Горизонт — целое число от ${MIN_HORIZON} до ${MAX_HORIZON}.`;
    }
    return null;
}

function toRequest(form: Form): ChangeSettingsRequest {
    return {
        address: form.address.trim(),
        phone: form.phone.trim(),
        workInterval: {start: form.workStart, end: form.workEnd},
        breakInterval: form.breakOn ? {start: form.breakStart, end: form.breakEnd} : null,
        generationEnabled: form.generationEnabled,
        horizonDays: Number(form.horizonDays),
        weekendDays: form.weekends,
    };
}

export default function SettingsSection() {
    const [loading, setLoading] = useState(true);
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const [form, setForm] = useState<Form | null>(null);
    const [confirmOpen, setConfirmOpen] = useState(false);

    useEffect(() => {
        settingsApi.get()
            .then(settings => setForm(fromSettings(settings)))
            .catch(() => setError("Не удалось загрузить настройки"))
            .finally(() => setLoading(false));
    }, []);

    async function save() {
        if (!form) return;
        const message = validate(form);
        if (message) {
            setSuccess(null);
            setError(message);
            return;
        }
        setBusy(true);
        setError(null);
        setSuccess(null);
        try {
            await settingsApi.change(toRequest(form));
            setSuccess("Настройки сохранены");
        } catch (err) {
            setError(err instanceof Error ? err.message : "Не удалось сохранить настройки");
        } finally {
            setBusy(false);
        }
    }

    async function apply() {
        setBusy(true);
        setError(null);
        setSuccess(null);
        try {
            const result = await settingsApi.apply();
            setConfirmOpen(false);
            setSuccess(
                `Применено к будущим дням: изменено ${result.affectedDays} ` +
                plural(result.affectedDays, "день", "дня", "дней")
            );
        } catch (err) {
            setConfirmOpen(false);
            setError(err instanceof Error ? err.message : "Не удалось применить настройки");
        } finally {
            setBusy(false);
        }
    }

    function toggleWeekend(day: DayOfWeek) {
        setForm(current =>
            current
                ? {
                    ...current,
                    weekends: current.weekends.includes(day)
                        ? current.weekends.filter(value => value !== day)
                        : [...current.weekends, day],
                }
                : current
        );
    }

    if (loading) {
        return <Stack sx={{alignItems: "center", py: 2}}><CircularProgress size={22}/></Stack>;
    }

    if (!form) {
        return <Stack spacing={2}>{error && <Alert severity="error">{error}</Alert>}</Stack>;
    }

    return (
        <Stack spacing={2}>
            {error && <Alert severity="error" onClose={() => setError(null)}>{error}</Alert>}
            {success && <Alert severity="success" onClose={() => setSuccess(null)}>{success}</Alert>}

            <Paper sx={cardSx}>
                <Stack spacing={2}>
                    <Typography variant="overline" sx={{color: "text.secondary", letterSpacing: "0.12em"}}>
                        Контакты
                    </Typography>
                    <TextField
                        label="Адрес"
                        value={form.address}
                        onChange={e => setForm(current => current && {...current, address: e.target.value})}
                        slotProps={{htmlInput: {maxLength: 200}}}
                        fullWidth
                    />
                    <TextField
                        label="Телефон"
                        value={form.phone}
                        onChange={e => setForm(current => current && {...current, phone: e.target.value})}
                        placeholder="+79991234567"
                        fullWidth
                    />
                </Stack>
            </Paper>

            <Paper sx={cardSx}>
                <Stack spacing={2}>
                    <Typography variant="overline" sx={{color: "text.secondary", letterSpacing: "0.12em"}}>
                        Генерация расписания
                    </Typography>
                    <FormControlLabel
                        control={
                            <Switch
                                checked={form.generationEnabled}
                                onChange={e => setForm(current => current && {...current, generationEnabled: e.target.checked})}
                            />
                        }
                        label="Генерировать дни"
                    />
                    <Stack direction="row" spacing={2}>
                        <TimeField
                            label="Начало"
                            value={form.workStart}
                            onChange={value => setForm(current => current && {...current, workStart: value})}
                        />
                        <TimeField
                            label="Конец"
                            value={form.workEnd}
                            onChange={value => setForm(current => current && {...current, workEnd: value})}
                        />
                    </Stack>
                    <FormControlLabel
                        control={
                            <Checkbox
                                checked={form.breakOn}
                                onChange={e => setForm(current => current && {...current, breakOn: e.target.checked})}
                            />
                        }
                        label="Перерыв"
                    />
                    {form.breakOn && (
                        <Stack direction="row" spacing={2}>
                            <TimeField
                                label="Перерыв с"
                                value={form.breakStart}
                                onChange={value => setForm(current => current && {...current, breakStart: value})}
                            />
                            <TimeField
                                label="Перерыв до"
                                value={form.breakEnd}
                                onChange={value => setForm(current => current && {...current, breakEnd: value})}
                            />
                        </Stack>
                    )}
                    <TextField
                        label="Горизонт, дней"
                        type="number"
                        value={form.horizonDays}
                        onChange={e => setForm(current => current && {...current, horizonDays: e.target.value})}
                        slotProps={{htmlInput: {min: MIN_HORIZON, max: MAX_HORIZON, step: 1}}}
                        fullWidth
                    />
                    <Stack spacing={1}>
                        <Typography variant="body2" color="text.secondary">Выходные дни</Typography>
                        <Stack direction="row" sx={{flexWrap: "wrap", columnGap: 1}}>
                            {WEEKDAYS.map(day => (
                                <FormControlLabel
                                    key={day.value}
                                    control={
                                        <Checkbox
                                            checked={form.weekends.includes(day.value)}
                                            onChange={() => toggleWeekend(day.value)}
                                        />
                                    }
                                    label={day.label}
                                />
                            ))}
                        </Stack>
                    </Stack>
                </Stack>
            </Paper>

            <Stack direction={{xs: "column", sm: "row"}} spacing={1}>
                <Button variant="contained" disabled={busy} onClick={save}>
                    Сохранить
                </Button>
                <Button variant="outlined" disabled={busy} onClick={() => setConfirmOpen(true)}>
                    Применить к будущим дням
                </Button>
            </Stack>

            <Dialog open={confirmOpen} onClose={() => setConfirmOpen(false)} fullWidth maxWidth="xs">
                <DialogTitle>Применить к будущим дням?</DialogTitle>
                <DialogContent>
                    <DialogContentText>
                        Будущие дни без активных записей будут приведены к настройкам. Отсутствующие дни будут созданы.
                        Применяются сохранённые настройки.
                    </DialogContentText>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setConfirmOpen(false)}>Отмена</Button>
                    <Button variant="contained" disabled={busy} onClick={apply}>Применить</Button>
                </DialogActions>
            </Dialog>
        </Stack>
    );
}
