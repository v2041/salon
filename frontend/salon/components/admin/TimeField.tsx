"use client";

import {MenuItem, TextField} from "@mui/material";
import {parseRuTime} from "@/lib/format";

type Props = {
    label: string;
    value: string;
    onChange: (value: string) => void;
};

const START_MINUTES = 8 * 60;
const END_MINUTES = 22 * 60;
const STEP_MINUTES = 10;

const TIMES: string[] = [];
for (let t = START_MINUTES; t <= END_MINUTES; t += STEP_MINUTES) {
    TIMES.push(`${String(Math.floor(t / 60)).padStart(2, "0")}:${String(t % 60).padStart(2, "0")}`);
}

export default function TimeField({label, value, onChange}: Props) {
    return (
        <TextField
            select
            label={label}
            value={parseRuTime(value) ?? value}
            onChange={e => onChange(e.target.value)}
            slotProps={{inputLabel: {shrink: true}}}
            fullWidth
        >
            {TIMES.map(t => (
                <MenuItem key={t} value={t}>{t}</MenuItem>
            ))}
        </TextField>
    );
}
