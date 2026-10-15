import {Stack, Typography} from "@mui/material";
import MyAppointmentsList from "@/components/MyAppointmentsList";

export default function MyAppointmentsPage() {
    return (
        <Stack spacing={2.5}>
            <Stack spacing={0.5}>
                <Typography variant="overline" sx={{color: "secondary.main", letterSpacing: "0.16em"}}>
                    История
                </Typography>
                <Typography variant="h5">Мои записи</Typography>
            </Stack>
            <MyAppointmentsList/>
        </Stack>
    );
}
