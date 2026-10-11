import {Box, Button, Paper, Stack, Typography} from "@mui/material";
import {formatPrice} from "@/lib/format";

type Props = {
    total: number;
    durationLabel: string;
    disabled: boolean;
    loading: boolean;
    onSubmit: () => void;
};

export default function BookingSummaryBar({total, durationLabel, disabled, loading, onSubmit}: Props) {
    return (
        <Box sx={{position: "fixed", bottom: 56, left: 0, right: 0, px: 2, pb: 1, zIndex: 1100}}>
            <Paper elevation={8} sx={{borderRadius: 2, p: 2}}>
                <Stack direction="row" sx={{alignItems: "center", justifyContent: "space-between"}}>
                    <Box>
                        <Typography variant="body2" color="text.secondary">{durationLabel}</Typography>
                        <Typography variant="h6">{formatPrice(total)}</Typography>
                    </Box>
                    <Button variant="contained" disabled={disabled} loading={loading} onClick={onSubmit}>
                        Записаться
                    </Button>
                </Stack>
            </Paper>
        </Box>
    );
}
