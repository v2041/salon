"use client";

import {Logout as LogoutIcon} from "@mui/icons-material";
import {
    AppBar,
    Box,
    Button,
    ButtonBase,
    CircularProgress,
    IconButton,
    Stack,
    Toolbar,
    Tooltip,
} from "@mui/material";
import Link from "next/link";
import {usePathname} from "next/navigation";
import {useState} from "react";
import LoginScreen from "@/components/LoginScreen";
import {useAuth} from "@/lib/auth";
import {UserRole} from "@/lib/types";

const TABS = [
    {path: "/booking", label: "Записаться"},
    {path: "/my-appointments", label: "Мои записи"},
];

const ADMIN_TAB = {path: "/admin", label: "Панель"};

// Гость видит эту страницу без входа — требование модерации VK ID.
const GUEST_PATH = "/booking";

export default function AppLayout({children}: {children: React.ReactNode}) {
    const {user, loading, signOut} = useAuth();
    const pathname = usePathname();
    const [showLogin, setShowLogin] = useState(false);

    if (loading) {
        return (
            <Stack sx={{minHeight: "100vh", alignItems: "center", justifyContent: "center"}}>
                <CircularProgress/>
            </Stack>
        );
    }

    if (!user && showLogin) return <LoginScreen onBack={() => setShowLogin(false)}/>;
    if (!user && pathname !== GUEST_PATH) return <LoginScreen/>;

    const tabs = !user
        ? TABS.filter(t => t.path === GUEST_PATH)
        : user.role === UserRole.Admin ? [...TABS, ADMIN_TAB] : TABS;

    return (
        <>
            <AppBar position="sticky">
                <Toolbar sx={{gap: 1.5, width: "100%", maxWidth: 560, mx: "auto", px: 2, minHeight: 64}}>
                    <Stack
                        component="nav"
                        direction="row"
                        spacing={0.25}
                        sx={{bgcolor: "rgba(255,255,255,0.08)", p: 0.5, borderRadius: 999}}
                    >
                        {tabs.map(t => {
                            const active = pathname === t.path;
                            return (
                                <ButtonBase
                                    key={t.path}
                                    component={Link}
                                    href={t.path}
                                    sx={{
                                        px: 1.5,
                                        py: 0.75,
                                        borderRadius: 999,
                                        fontSize: 13.5,
                                        fontWeight: active ? 700 : 500,
                                        color: active ? "#FFFFFF" : "rgba(255,255,255,0.6)",
                                        bgcolor: active ? "rgba(255,255,255,0.16)" : "transparent",
                                        transition: "background-color .2s, color .2s",
                                    }}
                                >
                                    {t.label}
                                </ButtonBase>
                            );
                        })}
                    </Stack>
                    {user ? (
                        <Tooltip title="Выход">
                            <IconButton onClick={signOut} sx={{color: "rgba(255,255,255,0.65)"}}>
                                <LogoutIcon fontSize="small"/>
                            </IconButton>
                        </Tooltip>
                    ) : (
                        <Button
                            size="small"
                            onClick={() => setShowLogin(true)}
                            sx={{
                                borderRadius: 999,
                                px: 1.5,
                                py: 0.75,
                                fontSize: 13.5,
                                fontWeight: 700,
                                color: "#FFFFFF",
                                bgcolor: "rgba(255,255,255,0.16)",
                                "&:hover": {bgcolor: "rgba(255,255,255,0.24)"},
                            }}
                        >
                            Войти
                        </Button>
                    )}
                </Toolbar>
            </AppBar>
            <Box sx={{maxWidth: 560, mx: "auto", px: 2, pt: 3, pb: 6}}>{children}</Box>
        </>
    );
}
