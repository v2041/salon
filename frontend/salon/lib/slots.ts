import {minutesToTime, timeToMinutes} from "./format";
import type {TimeInterval} from "./types";

const STEP_MINUTES = 30;

export function buildSlots(intervals: TimeInterval[], totalDuration: number) {
    const slots: string[] = [];
    for (const interval of intervals) {
        for (
            let t = timeToMinutes(interval.start);
            t + totalDuration <= timeToMinutes(interval.end);
            t += STEP_MINUTES
        ) {
            slots.push(minutesToTime(t));
        }
    }
    return slots;
}
