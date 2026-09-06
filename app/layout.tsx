import {AppRouterCacheProvider} from "@mui/material-nextjs/v13-appRouter";

export default function RootLayout({children}: {children: React.ReactNode}) {
    return (
        <html>
        <body>
        <AppRouterCacheProvider>
            {children}
        </AppRouterCacheProvider>
        </body>
        </html>
    );
}