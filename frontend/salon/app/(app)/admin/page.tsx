"use client";

import {Alert, ButtonBase, Stack, Typography} from "@mui/material";
import {useState} from "react";
import AnalyticsSection from "@/components/admin/AnalyticsSection";
import AppointmentsSection from "@/components/admin/AppointmentsSection";
import CreateAppointmentSection from "@/components/admin/CreateAppointmentSection";
import ScheduleSection from "@/components/admin/ScheduleSection";
import {useAuth} from "@/lib/auth";
import {accentInk} from "@/lib/theme";
import {UserRole} from "@/lib/types";

const SECTIONS = [
    {key: "create", label: "Создать запись"},
    {key: "appointments", label: "Записи"},
    {key: "schedule", label: "График"},
    {key: "analytics", label: "Аналитика"},
] as const;

type SectionKey = (typeof SECTIONS)[number]["key"];

export default function AdminPage() {
    const {user} = useAuth();
    const [section, setSection] = useState<SectionKey>("create");

    if (user?.role !== UserRole.Admin) {
        return <Alert severity="warning">Раздел доступен только администратору</Alert>;
    }

    return (
        <Stack spacing={2.5}>
            <Stack spacing={0.5}>
                <Typography variant="overline" sx={{color: accentInk, letterSpacing: "0.16em"}}>
                    Мастерская
                </Typography>
                <Typography variant="h5">Админ-панель</Typography>
            </Stack>
            <Stack
                direction="row"
                spacing={0.5}
                sx={{bgcolor: "rgba(42,28,28,0.06)", p: 0.5, borderRadius: 999, alignSelf: "flex-start"}}
            >
                {SECTIONS.map(s => {
                    const active = section === s.key;
                    return (
                        <ButtonBase
                            key={s.key}
                            onClick={() => setSection(s.key)}
                            sx={{
                                px: 1.75,
                                py: 0.75,
                                borderRadius: 999,
                                fontSize: 13.5,
                                fontWeight: active ? 700 : 500,
                                color: active ? "#FFFFFF" : "text.secondary",
                                bgcolor: active ? "primary.main" : "transparent",
                                transition: "background-color .2s, color .2s",
                            }}
                        >
                            {s.label}
                        </ButtonBase>
                    );
                })}
            </Stack>
            {section === "create" && <CreateAppointmentSection/>}
            {section === "appointments" && <AppointmentsSection/>}
            {section === "schedule" && <ScheduleSection/>}
            {section === "analytics" && <AnalyticsSection/>}
        </Stack>
    );
}
