"use client";

import {usePathname} from "next/navigation";
import {BottomNavigation, BottomNavigationAction, Paper} from "@mui/material";
import {MenuBook, EventNote, EditCalendar} from "@mui/icons-material";

const ITEMS = [
    {path: "/offerings", label: "Услуги", icon: <MenuBook/>},
    {path: "/booking", label: "Записаться", icon: <EditCalendar/>},
    {path: "/my-appointments", label: "Мои записи", icon: <EventNote/>},
];

export default function BottomNavigationBar() {
    const pathname = usePathname();
    return (
        <Paper sx={{position: "fixed", bottom: 0, left: 0, right: 0}} elevation={3}>
            <BottomNavigation showLabels value={pathname}>
                {ITEMS.map(item => (
                    <BottomNavigationAction
                        key={item.path}
                        label={item.label}
                        icon={item.icon}
                        href={item.path}
                        value={item.path}
                    />
                ))}
            </BottomNavigation>
        </Paper>
    );
}
