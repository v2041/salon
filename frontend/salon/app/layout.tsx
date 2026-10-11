import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import "./globals.css";
import BottomNavigationBar from "@/components/BottomNavigationBar";
import {AppRouterCacheProvider} from "@mui/material-nextjs/v13-appRouter";
import {Box} from "@mui/material";

export default function RootLayout({
  children,
}:{
  children: React.ReactNode;}
) {
  return (
    <html>
      <body>
      <AppRouterCacheProvider>
          <Box sx={{pb: 12}}>
              {children}
          </Box>
          <BottomNavigationBar/>
      </AppRouterCacheProvider>
      </body>
    </html>
  );
}
