"use client";

import {Alert, CircularProgress, Stack} from "@mui/material";
import {useEffect, useState} from "react";
import OfferingList from "@/components/booking/OfferingList";
import {offeringsApi} from "@/lib/api";
import type {OfferingResponse} from "@/lib/types";

export default function OfferingsPage() {
    const [offerings, setOfferings] = useState<OfferingResponse[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        offeringsApi.getAll()
            .then(setOfferings)
            .catch((e: unknown) => setError(e instanceof Error ? e.message : "Не удалось загрузить услуги"))
            .finally(() => setLoading(false));
    }, []);

    return (
        <>
            <h1>Услуги</h1>
            <Stack spacing={3} sx={{pb: 4}}>
                {error && <Alert severity="error">{error}</Alert>}
                {loading ? (
                    <Stack sx={{alignItems: "center", py: 4}}>
                        <CircularProgress/>
                    </Stack>
                ) : offerings.length === 0 ? (
                    <Alert severity="info">Услуги пока не добавлены</Alert>
                ) : (
                    <OfferingList offerings={offerings}/>
                )}
            </Stack>
        </>
    );
}
