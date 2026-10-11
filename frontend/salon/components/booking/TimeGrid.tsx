import {Box, Button} from "@mui/material";

type Props = {
    slots: string[];
    selected: string | null;
    onSelect: (time: string) => void;
};

export default function TimeGrid({slots, selected, onSelect}: Props) {
    return (
        <Box sx={{display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: 1}}>
            {slots.map(t => (
                <Button key={t} size="small" variant={selected === t ? "contained" : "outlined"} onClick={() => onSelect(t)}>
                    {t}
                </Button>
            ))}
        </Box>
    );
}
