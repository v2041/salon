"use client";

import {Alert, Box, Button, Checkbox, FormControlLabel, Link, Paper, Stack, TextField, Typography} from "@mui/material";
import NextLink from "next/link";
import {useState} from "react";
import {authApi} from "@/lib/api";
import {useAuth} from "@/lib/auth";
import {accentInk} from "@/lib/theme";
import {VkIdWidget} from "./VkIdWidget";

type Props = {
    onBack?: () => void;
};

export default function LoginScreen({onBack}: Props) {
    const {refresh} = useAuth();
    const [consent, setConsent] = useState(false);
    const [checkingName, setCheckingName] = useState(false);
    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    async function handleVkSuccess(payload: {
        code: string;
        deviceId: string;
        codeVerifier: string;
        redirectUrl: string;
    }) {
        setLoading(true);
        setError(null);
        try {
            const {isNew} = await authApi.vkLogin({...payload, consent: true});
            if (isNew) {
                const me = await authApi.me();
                setFirstName(me.firstName);
                setLastName(me.lastName ?? "");
                setCheckingName(true);
                return;
            }
            await refresh();
        } catch (err) {
            setError(err instanceof Error ? err.message : "Не удалось войти через VK ID");
        } finally {
            setLoading(false);
        }
    }

    async function submitName() {
        setLoading(true);
        setError(null);
        try {
            await authApi.updateName({firstName: firstName.trim(), lastName: lastName.trim() || null});
            await refresh();
        } catch (err) {
            setError(err instanceof Error ? err.message : "Не удалось сохранить имя");
        } finally {
            setLoading(false);
        }
    }

    return (
        <Box sx={{minHeight: "100vh", display: "flex", alignItems: "center", justifyContent: "center", p: 2}}>
            <Paper sx={{p: 3.5, width: "100%", maxWidth: 400, borderRadius: "24px", border: 1, borderColor: "divider", boxShadow: "0 30px 70px -40px rgba(26,26,26,0.55)"}}>
                <Stack spacing={2.25}>
                    <Stack spacing={0.5}>
                        <Typography variant="overline" sx={{color: accentInk, letterSpacing: "0.16em"}}>
                            Онлайн-запись
                        </Typography>
                        <Typography variant="h5">Студия красоты</Typography>
                    </Stack>
                    {checkingName ? (
                        <>
                            <Typography variant="body2" color="text.secondary">
                                Проверьте имя — так мастер увидит вас в записях.
                            </Typography>
                            <TextField label="Имя" value={firstName} onChange={e => setFirstName(e.target.value)} fullWidth/>
                            <TextField label="Фамилия" value={lastName} onChange={e => setLastName(e.target.value)} fullWidth/>
                            <Button variant="contained" disabled={!firstName.trim() || loading} onClick={submitName}>
                                Продолжить
                            </Button>
                        </>
                    ) : (
                        <>
                            <Typography variant="body2" color="text.secondary">
                                Войдите через VK ID. Имя и телефон придут из вашего профиля VK.
                            </Typography>
                            <FormControlLabel
                                control={<Checkbox checked={consent} onChange={e => setConsent(e.target.checked)}/>}
                                label={
                                    <Typography variant="body2">
                                        Я согласен с{" "}
                                        <Link component={NextLink} href="/privacy" target="_blank" rel="noopener noreferrer">
                                            политикой обработки персональных данных
                                        </Link>
                                    </Typography>
                                }
                            />
                            {consent && <VkIdWidget onSuccess={handleVkSuccess} onError={setError}/>}
                        </>
                    )}
                    {error && <Alert severity="error">{error}</Alert>}
                    {onBack && !checkingName && (
                        <Button variant="text" onClick={onBack}>
                            Продолжить без входа
                        </Button>
                    )}
                </Stack>
            </Paper>
        </Box>
    );
}
