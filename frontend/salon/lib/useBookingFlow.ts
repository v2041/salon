"use client";

import {useEffect, useState} from "react";
import {offeringsApi, schedulesApi} from "@/lib/api";
import {durationToMinutes} from "@/lib/format";
import {buildSlots} from "@/lib/slots";
import type {OfferingResponse, TimeInterval} from "@/lib/types";

export function useBookingFlow(initialSelectedIds: string[] = []) {
    const [offerings, setOfferings] = useState<OfferingResponse[]>([]);
    const [offeringsLoading, setOfferingsLoading] = useState(true);
    const [selectedIds, setSelectedIds] = useState<string[]>(initialSelectedIds);
    const [dates, setDates] = useState<string[]>([]);
    const [datesLoading, setDatesLoading] = useState(false);
    const [date, setDate] = useState<string | null>(null);
    const [intervals, setIntervals] = useState<TimeInterval[]>([]);
    const [slotsLoading, setSlotsLoading] = useState(false);
    const [time, setTime] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        offeringsApi.getAll()
            .then(setOfferings)
            .catch(() => setError("Не удалось загрузить услуги"))
            .finally(() => setOfferingsLoading(false));
    }, []);

    useEffect(() => {
        if (selectedIds.length === 0) return;
        let cancelled = false;
        schedulesApi.getAvailableDates(selectedIds)
            .then(res => {
                if (!cancelled) setDates(res.dates);
            })
            .catch(() => {
                if (!cancelled) setDates([]);
            })
            .finally(() => {
                if (!cancelled) setDatesLoading(false);
            });
        return () => {
            cancelled = true;
        };
    }, [selectedIds]);

    useEffect(() => {
        if (!date) return;
        let cancelled = false;
        schedulesApi.getFreeTime(date)
            .then(res => {
                if (!cancelled) setIntervals(res.freeIntervals);
            })
            .catch(() => {
                if (!cancelled) setIntervals([]);
            })
            .finally(() => {
                if (!cancelled) setSlotsLoading(false);
            });
        return () => {
            cancelled = true;
        };
    }, [date]);

    const selected = offerings.filter(o => selectedIds.includes(o.id));
    const totalDuration = selected.reduce((sum, o) => sum + durationToMinutes(o.duration), 0);
    const slots = date ? buildSlots(intervals, totalDuration) : [];

    function toggle(id: string) {
        const next = selectedIds.includes(id)
            ? selectedIds.filter(x => x !== id)
            : [...selectedIds, id];
        setSelectedIds(next);
        setDates([]);
        setDate(null);
        setTime(null);
        setIntervals([]);
        setDatesLoading(next.length > 0);
    }

    function selectDate(next: string) {
        if (next === date) return;
        setDate(next);
        setTime(null);
        setIntervals([]);
        setSlotsLoading(true);
    }

    function reset() {
        setSelectedIds([]);
        setDates([]);
        setDate(null);
        setTime(null);
        setIntervals([]);
    }

    return {
        offerings,
        offeringsLoading,
        selectedIds,
        selected,
        totalDuration,
        dates,
        datesLoading,
        date,
        slots,
        slotsLoading,
        time,
        error,
        setError,
        setTime,
        toggle,
        selectDate,
        reset,
    };
}
