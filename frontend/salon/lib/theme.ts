"use client";

import {createTheme} from "@mui/material/styles";

export const cardShadow = "0 1px 2px rgba(42,28,28,0.05), 0 16px 40px -28px rgba(42,28,28,0.45)";
export const accentInk = "#AE6D77";
export const serviceZoneBorder = "rgba(201,138,147,0.6)";
export const darkSurface = "#2A1C1C";

export const cardSx = {
    p: 2,
    borderRadius: "20px",
    border: 1,
    borderColor: "divider",
    bgcolor: "background.paper",
    boxShadow: cardShadow,
};

const theme = createTheme({
    palette: {
        primary: {main: "#2A1C1C", contrastText: "#FFFFFF"},
        secondary: {main: "#C98A93", contrastText: "#2A1C1C"},
        background: {default: "#FFFFFF", paper: "#FFFFFF"},
        text: {primary: "#241B1B", secondary: "#7A6A6A"},
        divider: "rgba(42,28,28,0.10)",
    },
    shape: {borderRadius: 16},
    typography: {
        fontFamily: 'var(--font-body), "Segoe UI", "Helvetica", sans-serif',
        h4: {fontWeight: 600, letterSpacing: "-0.02em", fontFamily: 'var(--font-display), Georgia, serif'},
        h5: {fontWeight: 600, letterSpacing: "-0.02em", fontFamily: 'var(--font-display), Georgia, serif'},
        h6: {fontWeight: 600, letterSpacing: "-0.01em", fontFamily: 'var(--font-display), Georgia, serif'},
        subtitle1: {letterSpacing: "-0.01em"},
        overline: {fontWeight: 700},
        button: {textTransform: "none", fontWeight: 600, letterSpacing: 0},
    },
    components: {
        MuiCssBaseline: {
            styleOverrides: {
                body: {
                    backgroundColor: "#FFFFFF",
                    minHeight: "100vh",
                    WebkitFontSmoothing: "antialiased",
                },
            },
        },
        MuiPaper: {
            defaultProps: {elevation: 0},
            styleOverrides: {
                root: {backgroundImage: "none"},
                outlined: {borderColor: "rgba(42,28,28,0.10)"},
            },
        },
        MuiAppBar: {
            defaultProps: {elevation: 0},
            styleOverrides: {
                root: {
                    backgroundColor: "#000000",
                    borderBottom: "1px solid rgba(255,255,255,0.10)",
                },
            },
        },
        MuiButton: {
            styleOverrides: {
                root: {borderRadius: 12, paddingInline: 18},
                contained: {boxShadow: "0 12px 28px -16px rgba(42,28,28,0.7)"},
            },
        },
        MuiAlert: {styleOverrides: {root: {borderRadius: 14}}},
    },
});

export default theme;
