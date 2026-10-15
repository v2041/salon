"use client";

import {createTheme} from "@mui/material/styles";

export const cardShadow = "0 1px 2px rgba(26,26,26,0.05), 0 16px 40px -28px rgba(26,26,26,0.45)";

const theme = createTheme({
    palette: {
        primary: {main: "#191714", contrastText: "#FFFFFF"},
        secondary: {main: "#A9835C", contrastText: "#FFFFFF"},
        background: {default: "#EDE4D7", paper: "#FFFFFF"},
        text: {primary: "#1B1916", secondary: "#6E655A"},
        divider: "rgba(26,26,26,0.09)",
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
                    backgroundColor: "#EDE4D7",
                    backgroundImage:
                        "radial-gradient(1200px 560px at 50% -10%, #FFFEFA 0%, #F5EEE4 45%, #E7DCCB 100%)",
                    backgroundAttachment: "fixed",
                    minHeight: "100vh",
                    WebkitFontSmoothing: "antialiased",
                },
            },
        },
        MuiPaper: {
            defaultProps: {elevation: 0},
            styleOverrides: {
                root: {backgroundImage: "none"},
                outlined: {borderColor: "rgba(26,26,26,0.09)"},
            },
        },
        MuiAppBar: {
            defaultProps: {elevation: 0},
            styleOverrides: {
                root: {
                    backgroundColor: "rgba(21,20,18,0.9)",
                    backdropFilter: "blur(14px)",
                    borderBottom: "1px solid rgba(255,255,255,0.08)",
                },
            },
        },
        MuiButton: {
            styleOverrides: {
                root: {borderRadius: 12, paddingInline: 18},
                contained: {boxShadow: "0 12px 28px -16px rgba(25,23,20,0.85)"},
            },
        },
        MuiAlert: {styleOverrides: {root: {borderRadius: 14}}},
    },
});

export default theme;
