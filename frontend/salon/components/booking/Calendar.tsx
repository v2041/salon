import {ChevronLeft as ChevronLeftIcon, ChevronRight as ChevronRightIcon} from "@mui/icons-material";
import {Box, IconButton, Paper, Stack, Typography} from "@mui/material";
import {useState} from "react";
import {toIsoDate} from "@/lib/format";

type Props = {
    selected: string | null;
    onSelect: (date: string) => void;
    availableDates?: string[];
    disablePast?: boolean;
};

const WEEKDAYS = ["Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс"];

function parseIso(iso: string) {
    const [y, m, d] = iso.split("-").map(Number);
    return new Date(y, m - 1, d);
}

function monthStart(iso: string) {
    const d = parseIso(iso);
    return new Date(d.getFullYear(), d.getMonth(), 1);
}

export default function Calendar({selected, onSelect, availableDates, disablePast = false}: Props) {
    const hasList = availableDates !== undefined;
    const sorted = hasList ? [...availableDates].sort() : [];

    const [month, setMonth] = useState(() => {
        if (selected) return monthStart(selected);
        if (sorted.length > 0) return monthStart(sorted[0]);
        const now = new Date();
        return new Date(now.getFullYear(), now.getMonth(), 1);
    });

    if (hasList && sorted.length === 0) return null;

    const minMonth = hasList ? monthStart(sorted[0]).getTime() : null;
    const maxMonth = hasList ? monthStart(sorted[sorted.length - 1]).getTime() : null;
    const atMin = minMonth !== null && month.getTime() <= minMonth;
    const atMax = maxMonth !== null && month.getTime() >= maxMonth;

    function shiftMonth(delta: number) {
        setMonth(m => new Date(m.getFullYear(), m.getMonth() + delta, 1));
    }

    const year = month.getFullYear();
    const mon = month.getMonth();
    const daysInMonth = new Date(year, mon + 1, 0).getDate();
    const offset = (new Date(year, mon, 1).getDay() + 6) % 7;
    const cells: (string | null)[] = [
        ...Array.from({length: offset}, () => null),
        ...Array.from({length: daysInMonth}, (_, i) => toIsoDate(new Date(year, mon, i + 1))),
    ];
    while (cells.length % 7 !== 0) cells.push(null);

    const availableSet = hasList ? new Set(sorted) : null;
    const todayIso = toIsoDate(new Date());

    function isAvailable(iso: string) {
        if (availableSet && !availableSet.has(iso)) return false;
        return !(disablePast && iso < todayIso);
    }

    function renderCell(iso: string) {
        const available = isAvailable(iso);
        const isSelected = iso === selected;
        return (
            <Box
                key={iso}
                onClick={available ? () => onSelect(iso) : undefined}
                sx={{
                    height: 40,
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    borderRadius: 2,
                    cursor: available ? "pointer" : "default",
                    color: isSelected ? "common.white" : available ? "text.primary" : "text.disabled",
                    bgcolor: isSelected ? "primary.main" : "transparent",
                }}
            >
                <Typography variant="body2">{Number(iso.split("-")[2])}</Typography>
            </Box>
        );
    }

    return (
        <Paper variant="outlined" elevation={1} sx={{borderRadius: 2, p: 2, borderColor: "text.disabled"}}>
            <Stack direction="row" sx={{justifyContent: "space-between", alignItems: "center", mb: 1}}>
                <IconButton size="small" disabled={atMin} onClick={() => shiftMonth(-1)}>
                    <ChevronLeftIcon/>
                </IconButton>
                <Typography variant="subtitle1" sx={{fontWeight: 600, textTransform: "capitalize"}}>
                    {month.toLocaleDateString("ru-RU", {month: "long", year: "numeric"})}
                </Typography>
                <IconButton size="small" disabled={atMax} onClick={() => shiftMonth(1)}>
                    <ChevronRightIcon/>
                </IconButton>
            </Stack>
            <Box sx={{display: "grid", gridTemplateColumns: "repeat(7, 1fr)", gap: 0.5}}>
                {WEEKDAYS.map(w => (
                    <Typography key={w} variant="body2" color="text.secondary" sx={{textAlign: "center"}}>
                        {w}
                    </Typography>
                ))}
                {cells.map((iso, i) => iso === null ? <Box key={`empty-${i}`}/> : renderCell(iso))}
            </Box>
        </Paper>
    );
}
