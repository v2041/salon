"use client";

import {ArrowBack as ArrowBackIcon} from "@mui/icons-material";
import {Alert, Button, CircularProgress, IconButton, Paper, Stack, Typography} from "@mui/material";
import {useParams, useRouter} from "next/navigation";
import {useEffect, useState} from "react";
import {offeringsApi} from "@/lib/api";
import {durationToMinutes, formatCategory, formatDurationValue, formatPrice} from "@/lib/format";
import {accentInk, cardSx} from "@/lib/theme";
import type {OfferingResponse} from "@/lib/types";

export default function OfferingPage() {
    const params = useParams<{id: string}>();
    const router = useRouter();
    const [offering, setOffering] = useState<OfferingResponse | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const id = params?.id;
        if (!id) return;
        let cancelled = false;
        offeringsApi.get(id)
            .then(res => {
                if (!cancelled) setOffering(res);
            })
            .catch(() => {
                if (!cancelled) setError("Услуга не найдена");
            })
            .finally(() => {
                if (!cancelled) setLoading(false);
            });
        return () => {
            cancelled = true;
        };
    }, [params?.id]);

    if (loading) {
        return <Stack sx={{alignItems: "center", py: 4}}><CircularProgress size={24}/></Stack>;
    }

    if (error || !offering) {
        return <Alert severity="error">{error ?? "Услуга не найдена"}</Alert>;
    }

    return (
        <Stack spacing={2.5}>
            <Stack direction="row" spacing={1} sx={{alignItems: "center"}}>
                <IconButton onClick={() => router.back()} aria-label="Назад">
                    <ArrowBackIcon/>
                </IconButton>
                <Typography variant="overline" sx={{color: accentInk, letterSpacing: "0.16em"}}>
                    {formatCategory(offering.category)}
                </Typography>
            </Stack>
            <Paper sx={{...cardSx, p: 3}}>
                <Stack spacing={1.5}>
                    <Typography variant="h5">{offering.title}</Typography>
                    <Typography variant="body2" color="text.secondary">{offering.description}</Typography>
                    <Stack direction="row" spacing={2} sx={{alignItems: "baseline"}}>
                        <Typography variant="subtitle1" sx={{fontWeight: 700}}>
                            {formatPrice(offering.price.value)}
                        </Typography>
                        <Typography variant="body2" color="text.secondary">
                            {formatDurationValue(durationToMinutes(offering.duration))}
                        </Typography>
                    </Stack>
                </Stack>
            </Paper>
            <Button
                variant="contained"
                size="large"
                sx={{py: 1.5}}
                onClick={() => router.push(`/booking?offeringId=${offering.id}`)}
            >
                Записаться
            </Button>
        </Stack>
    );
}
