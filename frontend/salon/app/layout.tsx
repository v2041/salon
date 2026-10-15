import type {Metadata, Viewport} from "next";
import "./globals.css";
import {Manrope, Playfair_Display} from "next/font/google";
import {AppRouterCacheProvider} from "@mui/material-nextjs/v13-appRouter";
import {CssBaseline, ThemeProvider} from "@mui/material";
import theme from "@/lib/theme";
import {AuthProvider} from "@/lib/auth";

const body = Manrope({
    subsets: ["latin", "cyrillic"],
    variable: "--font-body",
    display: "swap",
});

const display = Playfair_Display({
    subsets: ["latin", "cyrillic"],
    variable: "--font-display",
    display: "swap",
});

export const metadata: Metadata = {
    title: "Салон красоты",
    description: "Онлайн-запись в салон красоты",
};

export const viewport: Viewport = {
    width: "device-width",
    initialScale: 1,
};

export default function RootLayout({children}: {children: React.ReactNode}) {
    return (
        <html lang="ru" className={`${body.variable} ${display.variable}`}>
            <body>
                <AppRouterCacheProvider>
                    <ThemeProvider theme={theme}>
                        <CssBaseline/>
                        <AuthProvider>{children}</AuthProvider>
                    </ThemeProvider>
                </AppRouterCacheProvider>
            </body>
        </html>
    );
}
