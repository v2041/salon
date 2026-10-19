"use client";

import {Alert, Box, Button, Paper, Stack, TextField, Typography} from "@mui/material";
import {useState} from "react";
import {authApi} from "@/lib/api";
import {useAuth} from "@/lib/auth";
import {accentInk} from "@/lib/theme";

export default function LoginScreen() {
    const {refresh} = useAuth();
    const [step, setStep] = useState<"phone" | "code">("phone");
    const [phone, setPhone] = useState("");
    const [code, setCode] = useState("");
    const [firstName, setFirstName] = useState("");
    const [lastName, setLastName] = useState("");
    const [isRegistered, setIsRegistered] = useState(true);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    async function sendCode() {
        setLoading(true);
        setError(null);
        try {
            const res = await authApi.sendCode({phone});
            setIsRegistered(res.isRegistered);
            setStep("code");
        } catch (err) {
            setError(err instanceof Error ? err.message : "Не удалось отправить код");
        } finally {
            setLoading(false);
        }
    }

    async function submit() {
        setLoading(true);
        setError(null);
        try {
            if (isRegistered) await authApi.login({phone, code});
            else await authApi.register({phone, code, firstName, lastName});
            await refresh();
        } catch (err) {
            setError(err instanceof Error ? err.message : "Неверный номер телефона или код");
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
                        <Typography variant="h5">Салон красоты</Typography>
                    </Stack>
                    {step === "phone" ? (
                        <>
                            <Typography variant="body2" color="text.secondary">
                                Введите номер телефона. Код подтверждения появится в базе данных.
                            </Typography>
                            <TextField
                                label="Телефон"
                                value={phone}
                                onChange={e => setPhone(e.target.value)}
                                placeholder="+79991234567"
                                fullWidth
                            />
                            <Button variant="contained" disabled={!phone || loading} onClick={sendCode}>
                                Получить код
                            </Button>
                        </>
                    ) : (
                        <>
                            <Typography variant="body2" color="text.secondary">
                                Введите код из базы данных.
                            </Typography>
                            <TextField label="Код" value={code} onChange={e => setCode(e.target.value)} fullWidth/>
                            {!isRegistered && (
                                <>
                                    <TextField label="Имя" value={firstName} onChange={e => setFirstName(e.target.value)} fullWidth/>
                                    <TextField label="Фамилия" value={lastName} onChange={e => setLastName(e.target.value)} fullWidth/>
                                </>
                            )}
                            <Button
                                variant="contained"
                                disabled={!code || loading || (!isRegistered && (!firstName || !lastName))}
                                onClick={submit}
                            >
                                {isRegistered ? "Войти" : "Зарегистрироваться"}
                            </Button>
                            <Button variant="text" onClick={() => { setStep("phone"); setError(null); }}>
                                Изменить номер
                            </Button>
                        </>
                    )}
                    {error && <Alert severity="error">{error}</Alert>}
                </Stack>
            </Paper>
        </Box>
    );
}
