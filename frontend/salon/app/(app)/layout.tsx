"use client";

import {Logout as LogoutIcon} from "@mui/icons-material";
import {
    AppBar,
    Avatar,
    Box,
    ButtonBase,
    CircularProgress,
    IconButton,
    Paper,
    Stack,
    Toolbar,
    Tooltip,
    Typography,
} from "@mui/material";
import Link from "next/link";
import {usePathname} from "next/navigation";
import LoginScreen from "@/components/LoginScreen";
import {useAuth} from "@/lib/auth";
import {UserRole} from "@/lib/types";

const TABS = [
    {path: "/booking", label: "Записаться"},
    {path: "/my-appointments", label: "Мои записи"},
];

const ADMIN_TAB = {path: "/admin", label: "Панель"};

export default function AppLayout({children}: {children: React.ReactNode}) {
    const {user, loading, signOut} = useAuth();
    const pathname = usePathname();

    if (loading) {
        return (
            <Stack sx={{minHeight: "100vh", alignItems: "center", justifyContent: "center"}}>
                <CircularProgress/>
            </Stack>
        );
    }

    if (!user) return <LoginScreen/>;

    const tabs = user.role === UserRole.Admin ? [...TABS, ADMIN_TAB] : TABS;

    return (
        <>
            <AppBar position="sticky">
                <Toolbar sx={{gap: 1.5, width: "100%", maxWidth: 560, mx: "auto", px: 2, minHeight: 64}}>
                    <Avatar
                        sx={{width: 30, height: 30, bgcolor: "secondary.main", fontSize: 15, fontWeight: 700}}
                    >
                        С
                    </Avatar>
                    <Typography variant="subtitle1" sx={{fontWeight: 700, letterSpacing: "0.01em"}}>
                        Салон
                    </Typography>
                    <Tooltip title="Выход">
                        <IconButton onClick={signOut} sx={{color: "rgba(255,255,255,0.65)", ml: "auto"}}>
                            <LogoutIcon fontSize="small"/>
                        </IconButton>
                    </Tooltip>
                </Toolbar>
            </AppBar>
            <Box sx={{maxWidth: 560, mx: "auto", px: 2, pt: 3, pb: 12}}>{children}</Box>
            <Paper
                component="nav"
                elevation={3}
                sx={{
                    position: "fixed",
                    left: 0,
                    right: 0,
                    bottom: 0,
                    borderTop: 1,
                    borderColor: "divider",
                    borderRadius: 0,
                }}
            >
                <Stack direction="row" sx={{maxWidth: 560, mx: "auto", px: 2, py: 1, gap: 1}}>
                    {tabs.map(t => {
                        const active = pathname === t.path;
                        return (
                            <ButtonBase
                                key={t.path}
                                component={Link}
                                href={t.path}
                                sx={{
                                    flex: 1,
                                    py: 1,
                                    borderRadius: 3,
                                    fontSize: 13.5,
                                    fontWeight: active ? 700 : 500,
                                    color: active ? "primary.main" : "text.secondary",
                                    bgcolor: active ? "rgba(42,28,28,0.06)" : "transparent",
                                    transition: "background-color .2s, color .2s",
                                }}
                            >
                                {t.label}
                            </ButtonBase>
                        );
                    })}
                </Stack>
            </Paper>
        </>
    );
}
