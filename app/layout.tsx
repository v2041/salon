'use client'
import React from "react";
import SimpleBottomNavigation from "@/components/layout/BottomNavigation";

export default function RootLayout({children}: { children: React.ReactNode }) {
    return (
        <html>
        <body>
        {children}
        <SimpleBottomNavigation></SimpleBottomNavigation>
        </body>
        </html>
    );
}