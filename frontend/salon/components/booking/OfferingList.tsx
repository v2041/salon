"use client";

import {ExpandLess as ExpandLessIcon, ExpandMore as ExpandMoreIcon} from "@mui/icons-material";
import {Box, Checkbox, Collapse, List, ListItem, ListItemButton, Paper, Stack, Typography} from "@mui/material";
import {useState} from "react";
import {durationToMinutes, formatCategory, formatDurationValue, formatPrice} from "@/lib/format";
import type {OfferingResponse} from "@/lib/types";

type Props = {
    offerings: OfferingResponse[];
    selectedIds?: string[];
    onToggle?: (id: string) => void;
};

export default function OfferingList({offerings, selectedIds = [], onToggle}: Props) {
    const [collapsed, setCollapsed] = useState<Record<string, boolean>>({});
    const selectable = onToggle !== undefined;

    const groups: {category: string; items: OfferingResponse[]}[] = [];
    for (const o of offerings) {
        const category = formatCategory(o.category);
        const g = groups.find(x => x.category === category);
        if (g) g.items.push(o);
        else groups.push({category, items: [o]});
    }

    return (
        <Stack spacing={2}>
            {groups.map(({category, items}) => {
                const selectedCount = items.filter(i => selectedIds.includes(i.id)).length;
                const isCollapsed = collapsed[category] ?? false;
                return (
                    <Paper key={category} variant="outlined" elevation={1} sx={{borderRadius: 2, p: 1, borderColor: "text.disabled"}}>
                        <Box
                            onClick={() => setCollapsed(c => ({...c, [category]: !isCollapsed}))}
                            sx={{display: "flex", alignItems: "center", justifyContent: "space-between", px: 1, py: 0.5, cursor: "pointer"}}
                        >
                            <Stack direction="row" spacing={1} sx={{alignItems: "center"}}>
                                <Typography variant="subtitle1" sx={{fontWeight: 600}}>{category}</Typography>
                                {selectable && selectedCount > 0 && (
                                    <Typography variant="body2" color="primary">{selectedCount}</Typography>
                                )}
                            </Stack>
                            {isCollapsed ? <ExpandMoreIcon fontSize="small"/> : <ExpandLessIcon fontSize="small"/>}
                        </Box>
                        <Collapse in={!isCollapsed}>
                            <List disablePadding sx={{pt: 0.5}}>
                                {items.map(o => {
                                    const checked = selectedIds.includes(o.id);
                                    const content = (
                                        <>
                                            {selectable && (
                                                <Checkbox
                                                    checked={checked}
                                                    tabIndex={-1}
                                                    onClick={e => e.stopPropagation()}
                                                    onChange={() => onToggle?.(o.id)}
                                                    slotProps={{input: {"aria-label": o.title}}}
                                                />
                                            )}
                                            <Box sx={{flexGrow: 1, minWidth: 0}}>
                                                <Typography variant="body2" sx={{fontWeight: 500}}>{o.title}</Typography>
                                                <Typography variant="caption" color="text.secondary" sx={{display: "block"}}>
                                                    {o.description}
                                                </Typography>
                                            </Box>
                                            <Stack direction="row" spacing={1} sx={{alignItems: "baseline", flexShrink: 0}}>
                                                <Typography variant="body2" color="text.secondary">{formatDurationValue(durationToMinutes(o.duration))}</Typography>
                                                <Typography variant="body2" sx={{fontWeight: 600, whiteSpace: "nowrap"}}>{formatPrice(o.price.value)}</Typography>
                                            </Stack>
                                        </>
                                    );
                                    if (!selectable) {
                                        return (
                                            <ListItem key={o.id} sx={{borderRadius: 1, "& + &": {mt: 0.5}}}>
                                                {content}
                                            </ListItem>
                                        );
                                    }
                                    return (
                                        <ListItemButton
                                            key={o.id}
                                            selected={checked}
                                            onClick={() => onToggle?.(o.id)}
                                            sx={{borderRadius: 1, "& + &": {mt: 0.5}}}
                                        >
                                            {content}
                                        </ListItemButton>
                                    );
                                })}
                            </List>
                        </Collapse>
                    </Paper>
                );
            })}
        </Stack>
    );
}
