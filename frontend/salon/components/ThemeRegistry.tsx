"use client";

import {CssBaseline, ThemeProvider} from "@mui/material";
import theme from "@/lib/theme";

export default function ThemeRegistry({children}: {children: React.ReactNode}) {
    return (
        <ThemeProvider theme={theme}>
            <CssBaseline/>
            {children}
        </ThemeProvider>
    );
}
