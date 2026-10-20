"use client";

import {
    Alert,
    Button,
    Chip,
    CircularProgress,
    Dialog,
    DialogActions,
    DialogContent,
    DialogContentText,
    DialogTitle,
    MenuItem,
    Paper,
    Stack,
    TextField,
    Typography,
} from "@mui/material";
import {useEffect, useState} from "react";
import {offeringsApi} from "@/lib/api";
import {durationToMinutes, formatCategory, formatDurationValue, formatPrice, minutesToDuration} from "@/lib/format";
import {cardSx} from "@/lib/theme";
import {Category, type OfferingResponse} from "@/lib/types";

const CATEGORIES = [Category.Hair, Category.Nails, Category.Face, Category.Other] as const;

type Form = {
    title: string;
    description: string;
    price: string;
    duration: string;
    category: Category;
};

const EMPTY_FORM: Form = {
    title: "",
    description: "",
    price: "",
    duration: "30",
    category: Category.Hair,
};

function fromOffering(offering: OfferingResponse): Form {
    return {
        title: offering.title,
        description: offering.description,
        price: String(offering.price.value),
        duration: String(durationToMinutes(offering.duration)),
        category: offering.category,
    };
}

function validate(form: Form) {
    if (!form.title.trim()) return "Введите название услуги.";
    if (form.title.trim().length > 50) return "Название — не больше 50 символов.";
    if (!form.description.trim()) return "Введите описание услуги.";
    const price = Number(form.price.replace(",", "."));
    if (!form.price.trim() || Number.isNaN(price) || price < 0) return "Укажите цену: число от 0.";
    const minutes = Number(form.duration);
    if (!Number.isInteger(minutes) || minutes < 11 || minutes > 1439) return "Длительность — от 11 до 1439 минут.";
    return null;
}

function toRequest(form: Form) {
    return {
        price: Number(form.price.replace(",", ".")),
        title: form.title.trim(),
        description: form.description.trim(),
        duration: minutesToDuration(Number(form.duration)),
        category: form.category,
    };
}

export default function OfferingsSection() {
    const [offerings, setOfferings] = useState<OfferingResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [busy, setBusy] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [editError, setEditError] = useState<string | null>(null);
    const [deleteError, setDeleteError] = useState<string | null>(null);
    const [form, setForm] = useState<Form>(EMPTY_FORM);
    const [editing, setEditing] = useState<OfferingResponse | null>(null);
    const [editForm, setEditForm] = useState<Form>(EMPTY_FORM);
    const [deleting, setDeleting] = useState<OfferingResponse | null>(null);

    function load() {
        return offeringsApi.getAll()
            .then(setOfferings)
            .catch(() => setError("Не удалось загрузить услуги"))
            .finally(() => setLoading(false));
    }

    useEffect(() => {
        load();
    }, []);

    async function create() {
        const message = validate(form);
        if (message) {
            setError(message);
            return;
        }
        setBusy(true);
        setError(null);
        try {
            await offeringsApi.create(toRequest(form));
            setForm(EMPTY_FORM);
            await load();
        } catch (err) {
            setError(err instanceof Error ? err.message : "Не удалось создать услугу");
        } finally {
            setBusy(false);
        }
    }

    function openEdit(offering: OfferingResponse) {
        setEditError(null);
        setEditing(offering);
        setEditForm(fromOffering(offering));
    }

    function closeEdit() {
        setEditing(null);
        setEditError(null);
    }

    async function saveEdit() {
        if (!editing) return;
        const message = validate(editForm);
        if (message) {
            setEditError(message);
            return;
        }
        setBusy(true);
        setEditError(null);
        try {
            await offeringsApi.change(editing.id, toRequest(editForm));
            closeEdit();
            await load();
        } catch (err) {
            setEditError(err instanceof Error ? err.message : "Не удалось изменить услугу");
        } finally {
            setBusy(false);
        }
    }

    function closeDelete() {
        setDeleting(null);
        setDeleteError(null);
    }

    async function remove() {
        if (!deleting) return;
        setBusy(true);
        setDeleteError(null);
        try {
            await offeringsApi.remove(deleting.id);
            closeDelete();
            await load();
        } catch (err) {
            setDeleteError(err instanceof Error ? err.message : "Не удалось удалить услугу");
        } finally {
            setBusy(false);
        }
    }

    function fields(value: Form, onChange: (next: Form) => void) {
        return (
            <>
                <TextField
                    label="Название"
                    value={value.title}
                    onChange={e => onChange({...value, title: e.target.value})}
                    slotProps={{htmlInput: {maxLength: 50}}}
                    fullWidth
                />
                <TextField
                    label="Описание"
                    value={value.description}
                    onChange={e => onChange({...value, description: e.target.value})}
                    multiline
                    minRows={2}
                    fullWidth
                />
                <Stack direction={{xs: "column", sm: "row"}} spacing={2}>
                    <TextField
                        label="Цена, ₽"
                        type="number"
                        value={value.price}
                        onChange={e => onChange({...value, price: e.target.value})}
                        slotProps={{htmlInput: {min: 0, step: "0.01"}}}
                        fullWidth
                    />
                    <TextField
                        label="Длительность, мин"
                        type="number"
                        value={value.duration}
                        onChange={e => onChange({...value, duration: e.target.value})}
                        slotProps={{htmlInput: {min: 11, max: 1439, step: 1}}}
                        fullWidth
                    />
                </Stack>
                <TextField
                    select
                    label="Категория"
                    value={value.category}
                    onChange={e => onChange({...value, category: Number(e.target.value) as Category})}
                    fullWidth
                >
                    {CATEGORIES.map(category => (
                        <MenuItem key={category} value={category}>
                            {formatCategory(category)}
                        </MenuItem>
                    ))}
                </TextField>
            </>
        );
    }

    return (
        <Stack spacing={2}>
            {error && <Alert severity="error" onClose={() => setError(null)}>{error}</Alert>}
            <Paper sx={cardSx}>
                <Stack spacing={2}>
                    <Typography variant="overline" sx={{color: "text.secondary", letterSpacing: "0.12em"}}>
                        Новая услуга
                    </Typography>
                    {fields(form, setForm)}
                    <Button variant="contained" disabled={busy} onClick={create}>
                        Добавить услугу
                    </Button>
                </Stack>
            </Paper>

            <Stack spacing={1.5}>
                <Typography variant="overline" sx={{color: "text.secondary", letterSpacing: "0.12em"}}>
                    Активные услуги
                </Typography>
                {loading && <Stack sx={{alignItems: "center", py: 2}}><CircularProgress size={22}/></Stack>}
                {!loading && !error && offerings.length === 0 && (
                    <Typography variant="body2" color="text.secondary">Услуг пока нет</Typography>
                )}
                {offerings.map(offering => (
                    <Paper key={offering.id} sx={cardSx}>
                        <Stack spacing={1}>
                            <Stack direction="row" spacing={1} sx={{alignItems: "center", flexWrap: "wrap"}}>
                                <Typography variant="subtitle1" sx={{fontWeight: 650}}>
                                    {offering.title}
                                </Typography>
                                <Chip size="small" label={formatCategory(offering.category)}/>
                            </Stack>
                            <Typography variant="body2" color="text.secondary">
                                {offering.description}
                            </Typography>
                            <Typography variant="body2">
                                {formatPrice(offering.price.value)}
                                {" · "}
                                {formatDurationValue(durationToMinutes(offering.duration))}
                            </Typography>
                            <Stack direction="row" spacing={1}>
                                <Button size="small" variant="outlined" disabled={busy} onClick={() => openEdit(offering)}>
                                    Изменить
                                </Button>
                                <Button
                                    size="small"
                                    variant="outlined"
                                    color="error"
                                    disabled={busy}
                                    onClick={() => {
                                        setDeleteError(null);
                                        setDeleting(offering);
                                    }}
                                >
                                    Удалить
                                </Button>
                            </Stack>
                        </Stack>
                    </Paper>
                ))}
            </Stack>

            <Dialog open={Boolean(editing)} onClose={closeEdit} fullWidth maxWidth="sm">
                <DialogTitle>Изменить услугу</DialogTitle>
                <DialogContent>
                    <Stack spacing={2} sx={{pt: 1}}>
                        {editError && <Alert severity="error">{editError}</Alert>}
                        {fields(editForm, setEditForm)}
                    </Stack>
                </DialogContent>
                <DialogActions>
                    <Button onClick={closeEdit}>Отмена</Button>
                    <Button variant="contained" disabled={busy} onClick={saveEdit}>Сохранить</Button>
                </DialogActions>
            </Dialog>

            <Dialog open={Boolean(deleting)} onClose={closeDelete} fullWidth maxWidth="xs">
                <DialogTitle>Удалить услугу?</DialogTitle>
                <DialogContent>
                    <Stack spacing={2}>
                        <DialogContentText>
                            Услуга «{deleting?.title}» скроется из списка услуг.
                        </DialogContentText>
                        {deleteError && <Alert severity="error">{deleteError}</Alert>}
                    </Stack>
                </DialogContent>
                <DialogActions>
                    <Button onClick={closeDelete}>Отмена</Button>
                    <Button color="error" variant="contained" disabled={busy} onClick={remove}>
                        Удалить
                    </Button>
                </DialogActions>
            </Dialog>
        </Stack>
    );
}
