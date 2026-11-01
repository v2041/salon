"use client";

import {Alert, Button, CircularProgress, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle, Paper, Stack, TextField} from "@mui/material";
import {useState} from "react";
import Calendar from "@/components/booking/Calendar";
import ServicePicker from "@/components/booking/ServicePicker";
import TimeGrid from "@/components/booking/TimeGrid";
import {ApiError, appointmentsApi} from "@/lib/api";
import {formatDayLabel, formatPrice, formatTimeOnly} from "@/lib/format";
import {cardSx, serviceZoneBorder} from "@/lib/theme";
import {useBookingFlow} from "@/lib/useBookingFlow";

function Loader() {
    return <Stack sx={{alignItems: "center", py: 2}}><CircularProgress size={22}/></Stack>;
}

export default function CreateAppointmentSection() {
    const [phone, setPhone] = useState("");
    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [needRegister, setNeedRegister] = useState(false);
    const [submitting, setSubmitting] = useState(false);
    const [created, setCreated] = useState<string | null>(null);
    const flow = useBookingFlow();

    async function submit() {
        if (!flow.date || !flow.time) return;
        setSubmitting(true);
        flow.setError(null);
        setCreated(null);
        try {
            const res = await appointmentsApi.createForUser({
                phone,
                firstName: firstName.trim() || undefined,
                lastName: lastName.trim() || undefined,
                date: flow.date,
                time: `${flow.time}:00`,
                offeringIds: flow.selectedIds,
            });
            setCreated(
                `${phone}: ${formatDayLabel(res.date)}, ` +
                `${formatTimeOnly(res.interval.start)}–${formatTimeOnly(res.interval.end)}, ` +
                `${formatPrice(res.price.value)}`
            );
            flow.reset();
            setNeedRegister(false);
            setFirstName("");
            setLastName("");
        } catch (err) {
            if (err instanceof ApiError && err.status === 404 && err.message.includes("не зарегистрирован")) {
                setNeedRegister(true);
            } else {
                flow.setError(err instanceof Error ? err.message : "Не удалось создать запись");
            }
        } finally {
            setSubmitting(false);
        }
    }

    return (
        <Stack spacing={2}>
            {created && <Alert severity="success">Запись создана — {created}</Alert>}
            {flow.error && <Alert severity="error">{flow.error}</Alert>}
            <TextField
                label="Телефон клиента"
                value={phone}
                onChange={e => setPhone(e.target.value)}
                placeholder="+79341234567"
                fullWidth
            />
            <Paper sx={{...cardSx, p: 1.5, border: 3, borderColor: serviceZoneBorder}}>
                <ServicePicker offerings={flow.offerings} selectedIds={flow.selectedIds} onToggle={flow.toggle}/>
            </Paper>
            {flow.datesLoading && <Loader/>}
            {!flow.datesLoading && flow.selectedIds.length > 0 && (
                flow.dates.length === 0
                    ? <Alert severity="info">Нет доступных дат</Alert>
                    : <Paper sx={cardSx}>
                        <Calendar availableDates={flow.dates} selected={flow.date} onSelect={flow.selectDate}/>
                    </Paper>
            )}
            {flow.date && (flow.slotsLoading
                ? <Loader/>
                : flow.slots.length === 0
                    ? <Alert severity="info">Нет свободных слотов</Alert>
                    : <Paper sx={cardSx}>
                        <TimeGrid slots={flow.slots} selected={flow.time} onSelect={flow.setTime}/>
                    </Paper>)}
            <Button
                variant="contained"
                size="large"
                disabled={!phone || flow.selectedIds.length === 0 || !flow.date || !flow.time || submitting}
                onClick={submit}
            >
                Создать запись
            </Button>
            <Dialog open={needRegister} onClose={() => setNeedRegister(false)} fullWidth maxWidth="xs">
                <DialogTitle>Новый клиент</DialogTitle>
                <DialogContent>
                    <Stack spacing={2} sx={{pt: 1}}>
                        <DialogContentText>
                            Клиент с телефоном {phone} не зарегистрирован. Введите имя и фамилию — система создаст карточку клиента и запись.
                        </DialogContentText>
                        {flow.error && <Alert severity="error">{flow.error}</Alert>}
                        <TextField
                            label="Имя"
                            value={firstName}
                            onChange={e => setFirstName(e.target.value)}
                            autoFocus
                            fullWidth
                        />
                        <TextField
                            label="Фамилия (необязательно)"
                            value={lastName}
                            onChange={e => setLastName(e.target.value)}
                            fullWidth
                        />
                    </Stack>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setNeedRegister(false)}>Отмена</Button>
                    <Button
                        variant="contained"
                        disabled={submitting || !firstName.trim()}
                        onClick={submit}
                    >
                        Зарегистрировать и создать
                    </Button>
                </DialogActions>
            </Dialog>
        </Stack>
    );
}
