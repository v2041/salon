"use client";

import {Alert, Box, Button, CircularProgress, Paper, Stack, Typography} from "@mui/material";
import {useState} from "react";
import {analyticsApi} from "@/lib/api";
import DateField from "@/components/admin/DateField";
import {formatMonthLabel, formatPrice, formatRuDate, parseRuDate, toIsoDate} from "@/lib/format";
import {cardSx} from "@/lib/theme";

type Row = {name: string; count: number; price: number};
type Client = {name: string; phoneNumber: string; spent: number};
type Month = {month: string; gain: number};
type Clients = {total: number; new: number; returning: number};
type Losses = {
    canceled: number;
    canceledRate: number;
    rejected: number;
    rejectedRate: number;
    missed: number;
    missedRate: number;
};

type Data = {
    gain: number;
    averageCheck: number;
    noShow: number;
    workload: number;
    popular: Row[];
    gainRows: Row[];
    best: Client[];
    months: Month[];
    clients: Clients;
    losses: Losses;
};

const EMPTY_CLIENTS: Clients = {total: 0, new: 0, returning: 0};
const EMPTY_LOSSES: Losses = {
    canceled: 0,
    canceledRate: 0,
    rejected: 0,
    rejectedRate: 0,
    missed: 0,
    missedRate: 0,
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

function Bar({label, value, max, caption}: {label: string; value: number; max: number; caption: string}) {
    const width = max > 0 ? Math.max(2, Math.round((value / max) * 100)) : 0;
    return (
        <Stack spacing={0.5}>
            <Stack direction="row" sx={{justifyContent: "space-between", gap: 2}}>
                <Typography variant="body2" color="text.secondary">{label}</Typography>
                <Typography variant="body2" sx={{whiteSpace: "nowrap"}}>{caption}</Typography>
            </Stack>
            <Box sx={{height: 8, borderRadius: 999, bgcolor: "divider", overflow: "hidden"}}>
                <Box sx={{height: "100%", width: `${width}%`, bgcolor: "primary.main"}}/>
            </Box>
        </Stack>
    );
}

function Panel({title, children}: {title: string; children: React.ReactNode}) {
    return (
        <Paper sx={cardSx}>
            <Stack spacing={1.25}>
                <Typography variant="overline" sx={{color: "text.secondary", letterSpacing: "0.12em"}}>
                    {title}
                </Typography>
                {children}
            </Stack>
        </Paper>
    );
}

export default function AnalyticsSection() {
    const [from, setFrom] = useState(() => formatRuDate(defaultFrom()));
    const [to, setTo] = useState(() => formatRuDate(toIsoDate(new Date())));
    const [data, setData] = useState<Data | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    async function load() {
        const fromIso = parseRuDate(from);
        const toIso = parseRuDate(to);
        if (!fromIso || !toIso) {
            setError("Проверьте даты: формат дд.мм.гггг");
            return;
        }
        setLoading(true);
        setError(null);
        try {
            const [gain, averageCheck, noShow, workload, popular, gainRows, best, months, clients, losses] =
                await Promise.all([
                    safe(analyticsApi.getGain(fromIso, toIso)),
                    safe(analyticsApi.getAverageCheck(fromIso, toIso)),
                    safe(analyticsApi.getNoShowRate(fromIso, toIso)),
                    safe(analyticsApi.getWorkloadRate(fromIso, toIso)),
                    safe(analyticsApi.getOfferingsByPopularity(fromIso, toIso)),
                    safe(analyticsApi.getOfferingsByGain(fromIso, toIso)),
                    safe(analyticsApi.getBestClients(5)),
                    safe(analyticsApi.getGainByMonth(fromIso, toIso)),
                    safe(analyticsApi.getClients(fromIso, toIso)),
                    safe(analyticsApi.getLosses(fromIso, toIso)),
                ]);
            setData({
                gain: gain?.gain.value ?? 0,
                averageCheck: averageCheck?.price.value ?? 0,
                noShow: noShow?.rate ?? 0,
                workload: workload?.rate ?? 0,
                popular: popular?.map(p => ({name: p.name, count: p.count, price: p.price.value})) ?? [],
                gainRows: gainRows?.map(p => ({name: p.name, count: p.count, price: p.price.value})) ?? [],
                best: best?.map(b => ({name: b.name, phoneNumber: b.phoneNumber, spent: b.spent.value})) ?? [],
                months: months?.months.map(m => ({month: m.month, gain: m.gain.value})) ?? [],
                clients: clients ?? EMPTY_CLIENTS,
                losses: losses ?? EMPTY_LOSSES,
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
                        <DateField label="С" value={from} onChange={setFrom}/>
                        <DateField label="По" value={to} onChange={setTo}/>
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
                        <Metric label="Загруженность" value={percent(data.workload)}/>
                        <Metric label="Пропуски" value={percent(data.noShow)}/>
                        <Metric
                            label="Отмены"
                            value={`${data.losses.canceled} · ${percent(data.losses.canceledRate)}`}
                        />
                        <Metric
                            label="Отказы"
                            value={`${data.losses.rejected} · ${percent(data.losses.rejectedRate)}`}
                        />
                        <Metric label="Всего клиентов" value={String(data.clients.total)}/>
                        <Metric label="Новые клиенты" value={String(data.clients.new)}/>
                    </Box>

                    <Panel title="N1 · Доход по месяцам">
                        {data.months.length === 0 && (
                            <Typography variant="body2" color="text.secondary">Нет данных</Typography>
                        )}
                        {data.months.map(m => (
                            <Bar
                                key={m.month}
                                label={formatMonthLabel(m.month)}
                                value={m.gain}
                                max={Math.max(...data.months.map(x => x.gain))}
                                caption={formatPrice(m.gain)}
                            />
                        ))}
                    </Panel>

                    <Panel title="N1 · Доход по услугам">
                        {data.gainRows.length === 0 && (
                            <Typography variant="body2" color="text.secondary">Нет данных</Typography>
                        )}
                        {data.gainRows.map(r => (
                            <Bar
                                key={r.name}
                                label={`${r.name} · ${r.count}`}
                                value={r.price}
                                max={Math.max(...data.gainRows.map(x => x.price))}
                                caption={formatPrice(r.price)}
                            />
                        ))}
                    </Panel>

                    <Panel title="N2 · Клиенты">
                        <Bar label="Всего" value={data.clients.total} max={data.clients.total} caption={String(data.clients.total)}/>
                        <Bar label="Новые" value={data.clients.new} max={data.clients.total} caption={String(data.clients.new)}/>
                        <Bar
                            label="Повторные"
                            value={data.clients.returning}
                            max={data.clients.total}
                            caption={String(data.clients.returning)}
                        />
                    </Panel>

                    <Panel title="N4 · Отмены, отказы и пропуски">
                        <Bar
                            label="Отмены"
                            value={data.losses.canceled}
                            max={Math.max(data.losses.canceled, data.losses.rejected, data.losses.missed)}
                            caption={`${data.losses.canceled} · ${percent(data.losses.canceledRate)}`}
                        />
                        <Bar
                            label="Отказы"
                            value={data.losses.rejected}
                            max={Math.max(data.losses.canceled, data.losses.rejected, data.losses.missed)}
                            caption={`${data.losses.rejected} · ${percent(data.losses.rejectedRate)}`}
                        />
                        <Bar
                            label="Пропуски"
                            value={data.losses.missed}
                            max={Math.max(data.losses.canceled, data.losses.rejected, data.losses.missed)}
                            caption={`${data.losses.missed} · ${percent(data.losses.missedRate)}`}
                        />
                    </Panel>

                    <Panel title="Популярные услуги">
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
                    </Panel>

                    <Panel title="Лучшие клиенты">
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
                    </Panel>
                </>
            )}
        </Stack>
    );
}
