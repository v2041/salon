"use client";

import {CheckCircle as CheckCircleIcon} from "@mui/icons-material";
import {ButtonBase, Stack, Typography} from "@mui/material";
import {durationToMinutes, formatCategory, formatDurationValue, formatPrice} from "@/lib/format";
import {cardShadow, accentInk} from "@/lib/theme";
import type {Category, OfferingResponse} from "@/lib/types";

type Props = {
    offerings: OfferingResponse[];
    selectedIds: string[];
    onToggle: (id: string) => void;
};

export default function ServicePicker({offerings, selectedIds, onToggle}: Props) {
    const groups: {category: Category; items: OfferingResponse[]}[] = [];
    for (const o of offerings) {
        const group = groups.find(g => g.category === o.category);
        if (group) group.items.push(o);
        else groups.push({category: o.category, items: [o]});
    }

    return (
        <Stack spacing={2.5}>
            {groups.map(g => (
                <Stack key={g.category} spacing={1.25}>
                    <Typography variant="overline" sx={{color: accentInk, letterSpacing: "0.14em"}}>
                        {formatCategory(g.category)}
                    </Typography>
                    {g.items.map(o => {
                        const selected = selectedIds.includes(o.id);
                        return (
                            <ButtonBase
                                key={o.id}
                                onClick={() => onToggle(o.id)}
                                sx={{
                                    display: "block",
                                    width: "100%",
                                    textAlign: "left",
                                    p: 2,
                                    borderRadius: "18px",
                                    border: 1,
                                    borderColor: selected ? "primary.main" : "divider",
                                    bgcolor: "background.paper",
                                    boxShadow: selected ? cardShadow : "none",
                                    transition: "border-color .2s, box-shadow .2s, transform .2s",
                                    "&:active": {transform: "scale(0.995)"},
                                }}
                            >
                                <Stack direction="row" spacing={2} sx={{alignItems: "center"}}>
                                    <Stack spacing={0.25} sx={{flex: 1, minWidth: 0}}>
                                        <Typography variant="subtitle1" sx={{fontWeight: 650}}>{o.title}</Typography>
                                        <Typography variant="body2" color="text.secondary">
                                            {formatDurationValue(durationToMinutes(o.duration))}
                                        </Typography>
                                    </Stack>
                                    <Stack spacing={0.5} sx={{alignItems: "flex-end"}}>
                                        <Typography variant="subtitle1" sx={{fontWeight: 700, whiteSpace: "nowrap"}}>
                                            {formatPrice(o.price.value)}
                                        </Typography>
                                        <CheckCircleIcon
                                            fontSize="small"
                                            sx={{color: selected ? accentInk : "rgba(36,27,27,0.15)"}}
                                        />
                                    </Stack>
                                </Stack>
                            </ButtonBase>
                        );
                    })}
                </Stack>
            ))}
        </Stack>
    );
}
