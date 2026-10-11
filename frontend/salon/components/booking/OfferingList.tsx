import {ExpandLess as ExpandLessIcon, ExpandMore as ExpandMoreIcon} from "@mui/icons-material";
import {Box, Checkbox, Collapse, List, ListItemButton, Paper, Stack, Typography} from "@mui/material";
import {useState} from "react";
import {formatDurationValue, formatPrice} from "@/lib/format";

export type Offering = {id: number; title: string; category: string; price: number; duration: number};

type Props = {
    offerings: Offering[];
    selectedIds: number[];
    onToggle: (id: number) => void;
};

export default function OfferingList({offerings, selectedIds, onToggle}: Props) {
    const [collapsed, setCollapsed] = useState<Record<string, boolean>>({});

    const groups: {category: string; items: Offering[]}[] = [];
    for (const o of offerings) {
        const g = groups.find(x => x.category === o.category);
        if (g) g.items.push(o);
        else groups.push({category: o.category, items: [o]});
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
                                {selectedCount > 0 && (
                                    <Typography variant="body2" color="primary">{selectedCount}</Typography>
                                )}
                            </Stack>
                            {isCollapsed ? <ExpandMoreIcon fontSize="small"/> : <ExpandLessIcon fontSize="small"/>}
                        </Box>
                        <Collapse in={!isCollapsed}>
                            <List disablePadding sx={{pt: 0.5}}>
                                {items.map(o => {
                                    const checked = selectedIds.includes(o.id);
                                    return (
                                        <ListItemButton key={o.id} selected={checked} onClick={() => onToggle(o.id)} sx={{borderRadius: 1, "& + &": {mt: 0.5}}}>
                                            <Checkbox
                                                checked={checked}
                                                tabIndex={-1}
                                                onClick={e => e.stopPropagation()}
                                                onChange={() => onToggle(o.id)}
                                                slotProps={{input: {"aria-label": o.title}}}
                                            />
                                            <Typography variant="body2" sx={{fontWeight: 500, flexGrow: 1}}>{o.title}</Typography>
                                            <Stack direction="row" spacing={1} sx={{alignItems: "baseline"}}>
                                                <Typography variant="body2" color="text.secondary">{formatDurationValue(o.duration)}</Typography>
                                                <Typography variant="body2" sx={{fontWeight: 600, whiteSpace: "nowrap"}}>{formatPrice(o.price)}</Typography>
                                            </Stack>
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
