"use client";

import {Alert, Box, Button, CircularProgress, Paper, Stack, TextField, Typography} from "@mui/material";
import {useState} from "react";
import {analyticsApi} from "@/lib/api";
import {formatPrice, toIsoDate} from "@/lib/format";
import {cardSx} from "@/lib/theme";

type Row = {name: string; count: number; price: number};
type Client = {name: string; phoneNumber: string; spent: number};

type Data = {
    gain: number;
    averageCheck: number;
    noShow: number;
    workload: number;
    popular: Row[];
    best: Client[];
};

function defaultFrom() {
    const d = new Date();
    d.setDate(d.getDate() - 30);
    return toIsoDate(d);
}

function safe<T>(promise: Promise<T>) {
    return promise.catch(() => null);
}

function percent(rate: number) {
    return `${(rate * 100).toLocaleString("ru-RU", {maximumFractionDigits: 1})} %`;
}

function Metric({label, value}: {label: string; value: string}) {
    return (
        <Paper sx={cardSx}>
            <Typography variant="caption" sx={{color: "text.secondary"}}>{label}</Typography>
            <Typography variant="h6">{value}</Typography>
        </Paper>
    );
}

export default function AnalyticsSection() {
    const [from, setFrom] = useState(defaultFrom);
    const [to, setTo] = useState(() => toIsoDate(new Date()));
    const [data, setData] = useState<Data | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    async function load() {
        setLoading(true);
        setError(null);
        try {
            const [gain, averageCheck, noShow, workload, popular, best] = await Promise.all([
                safe(analyticsApi.getGain(from, to)),
                safe(analyticsApi.getAverageCheck(from, to)),
                safe(analyticsApi.getNoShowRate(from, to)),
                safe(analyticsApi.getWorkloadRate(from, to)),
                safe(analyticsApi.getOfferingsByPopularity(from, to)),
                safe(analyticsApi.getBestClients(5)),
            ]);
            setData({
                gain: gain?.gain.value ?? 0,
                averageCheck: averageCheck?.price.value ?? 0,
                noShow: noShow?.rate ?? 0,
                workload: workload?.rate ?? 0,
                popular: popular?.map(p => ({name: p.name, count: p.count, price: p.price.value})) ?? [],
                best: best?.map(b => ({name: b.name, phoneNumber: b.phoneNumber, spent: b.spent.value})) ?? [],
            });
        } catch {
            setError("Не удалось загрузить аналитику");
        } finally {
            setLoading(false);
        }
    }

    return (
        <Stack spacing={2}>
            {error && <Alert severity="error">{error}</Alert>}
            <Paper sx={cardSx}>
                <Stack spacing={2}>
                    <Stack direction="row" spacing={2}>
                        <TextField
                            label="С"
                            type="date"
                            value={from}
                            onChange={e => setFrom(e.target.value)}
                            slotProps={{inputLabel: {shrink: true}}}
                            fullWidth
                        />
                        <TextField
                            label="По"
                            type="date"
                            value={to}
                            onChange={e => setTo(e.target.value)}
                            slotProps={{inputLabel: {shrink: true}}}
                            fullWidth
                        />
                    </Stack>
                    <Button variant="contained" disabled={!from || !to || loading} onClick={load}>
                        Показать
                    </Button>
                </Stack>
            </Paper>

            {loading && <Stack sx={{alignItems: "center", py: 2}}><CircularProgress size={22}/></Stack>}

            {!loading && !data && (
                <Typography variant="body2" color="text.secondary">
                    Выберите период и нажмите «Показать».
                </Typography>
            )}

            {!loading && data && (
                <>
                    <Box sx={{display: "grid", gridTemplateColumns: "1fr 1fr", gap: 2}}>
                        <Metric label="Доход за период" value={formatPrice(data.gain)}/>
                        <Metric label="Средний чек" value={formatPrice(data.averageCheck)}/>
                        <Metric label="Пропуски" value={percent(data.noShow)}/>
                        <Metric label="Загруженность" value={percent(data.workload)}/>
                    </Box>

                    <Paper sx={cardSx}>
                        <Stack spacing={1}>
                            <Typography variant="overline" sx={{color: "text.secondary", letterSpacing: "0.12em"}}>
                                Популярные услуги
                            </Typography>
                            {data.popular.length === 0 && (
                                <Typography variant="body2" color="text.secondary">Нет данных</Typography>
                            )}
                            {data.popular.map(r => (
                                <Stack key={r.name} direction="row" sx={{justifyContent: "space-between", gap: 2}}>
                                    <Typography variant="body2" color="text.secondary">
                                        {r.name} · {r.count}
                                    </Typography>
                                    <Typography variant="body2" sx={{whiteSpace: "nowrap"}}>
                                        {formatPrice(r.price)}
                                    </Typography>
                                </Stack>
                            ))}
                        </Stack>
                    </Paper>

                    <Paper sx={cardSx}>
                        <Stack spacing={1}>
                            <Typography variant="overline" sx={{color: "text.secondary", letterSpacing: "0.12em"}}>
                                Лучшие клиенты
                            </Typography>
                            {data.best.length === 0 && (
                                <Typography variant="body2" color="text.secondary">Нет данных</Typography>
                            )}
                            {data.best.map(c => (
                                <Stack key={c.phoneNumber} direction="row" sx={{justifyContent: "space-between", gap: 2}}>
                                    <Typography variant="body2" color="text.secondary">
                                        {c.name} · {c.phoneNumber}
                                    </Typography>
                                    <Typography variant="body2" sx={{whiteSpace: "nowrap"}}>
                                        {formatPrice(c.spent)}
                                    </Typography>
                                </Stack>
                            ))}
                        </Stack>
                    </Paper>
                </>
            )}
        </Stack>
    );
}
