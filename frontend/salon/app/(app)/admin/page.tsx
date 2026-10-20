"use client";

import {Alert, Box, Stack, Tab, Tabs, Typography} from "@mui/material";
import {useState} from "react";
import AnalyticsSection from "@/components/admin/AnalyticsSection";
import AppointmentsSection from "@/components/admin/AppointmentsSection";
import CreateAppointmentSection from "@/components/admin/CreateAppointmentSection";
import OfferingsSection from "@/components/admin/OfferingsSection";
import ScheduleSection from "@/components/admin/ScheduleSection";
import {useAuth} from "@/lib/auth";
import {accentInk} from "@/lib/theme";
import {UserRole} from "@/lib/types";

const SECTIONS = [
    {key: "create", label: "Создать запись"},
    {key: "offerings", label: "Услуги"},
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
            <Tabs
                value={section}
                onChange={(_, value) => setSection(value as SectionKey)}
                variant="scrollable"
                allowScrollButtonsMobile
                aria-label="Разделы админ-панели"
                sx={{
                    bgcolor: "rgba(42,28,28,0.06)",
                    p: 0.5,
                    borderRadius: 999,
                    alignSelf: "flex-start",
                    maxWidth: "100%",
                    minHeight: 0,
                    "& .MuiTabs-indicator": {display: "none"},
                    "& .MuiTabs-flexContainer": {gap: 0.5},
                }}
            >
                {SECTIONS.map(s => (
                    <Tab
                        key={s.key}
                        value={s.key}
                        label={s.label}
                        id={`admin-tab-${s.key}`}
                        aria-controls={`admin-panel-${s.key}`}
                        sx={{
                            px: 1.75,
                            py: 0.75,
                            minHeight: 0,
                            minWidth: 0,
                            borderRadius: 999,
                            fontSize: 13.5,
                            textTransform: "none",
                            fontWeight: 500,
                            color: "text.secondary",
                            transition: "background-color .2s, color .2s",
                            "&.Mui-selected": {
                                color: "#FFFFFF",
                                bgcolor: "primary.main",
                                fontWeight: 700,
                            },
                        }}
                    />
                ))}
            </Tabs>
            <Box role="tabpanel" id={`admin-panel-${section}`} aria-labelledby={`admin-tab-${section}`}>
                {section === "create" && <CreateAppointmentSection/>}
                {section === "offerings" && <OfferingsSection/>}
                {section === "appointments" && <AppointmentsSection/>}
                {section === "schedule" && <ScheduleSection/>}
                {section === "analytics" && <AnalyticsSection/>}
            </Box>
        </Stack>
    );
}
