"use client";

import {CalendarMonth as CalendarMonthIcon} from "@mui/icons-material";
import {Box, IconButton, InputAdornment, Popover, TextField} from "@mui/material";
import {useState} from "react";
import Calendar from "@/components/booking/Calendar";
import {formatRuDate, parseRuDate} from "@/lib/format";

type Props = {
    label: string;
    value: string;
    onChange: (value: string) => void;
    disablePast?: boolean;
};

export default function DateField({label, value, onChange, disablePast}: Props) {
    const [anchor, setAnchor] = useState<HTMLElement | null>(null);
    const selected = parseRuDate(value);

    return (
        <>
            <TextField
                label={label}
                value={value}
                onChange={e => onChange(e.target.value)}
                placeholder="дд.мм.гггг"
                slotProps={{
                    inputLabel: {shrink: true},
                    input: {
                        endAdornment: (
                            <InputAdornment position="end">
                                <IconButton edge="end" onClick={e => setAnchor(e.currentTarget)}>
                                    <CalendarMonthIcon/>
                                </IconButton>
                            </InputAdornment>
                        ),
                    },
                }}
                fullWidth
            />
            <Popover
                open={Boolean(anchor)}
                anchorEl={anchor}
                onClose={() => setAnchor(null)}
                anchorOrigin={{vertical: "bottom", horizontal: "left"}}
            >
                <Box sx={{p: 1.5, width: 336}}>
                    <Calendar
                        selected={selected}
                        disablePast={disablePast}
                        onSelect={next => {
                            onChange(formatRuDate(next));
                            setAnchor(null);
                        }}
                    />
                </Box>
            </Popover>
        </>
    );
}
