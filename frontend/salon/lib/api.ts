import {
    type AdminAppointmentResponse,
    type AppointmentDetailResponse,
    type AppointmentResponse,
    AppointmentStatus,
    type AvailableDatesResponse,
    type AverageCheckResponse,
    type BestClientResponse,
    type ChangeAppointmentStatusRequest,
    type ChangeAppointmentTimeRequest,
    type ChangeOfferingRequest,
    type ChangeScheduleRequest,
    type CreateAppointmentRequest,
    type CreateOfferingRequest,
    type CreateScheduleRequest,
    type CurrentUser,
    type FreeTimeResponse,
    type GainResponse,
    type LoginRequest,
    type Money,
    type OfferingResponse,
    type OfferingsStatResponse,
    type RateResponse,
    type RegisterRequest,
    type ScheduleResponse,
    type SendVerificationCodeRequest,
    type SendVerificationCodeResponse,
    type UserAppointmentResponse,
} from "./types";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5164";

export class ApiError extends Error {
    status: number;
    constructor(message: string, status: number) {
        super(message);
        this.status = status;
    }
}

export async function apiFetch<T>(
    endpoint: string,
    options: RequestInit = {}
): Promise<T> {
    const res = await fetch(`${API_URL}${endpoint}`, {
        ...options,
        credentials: "include",
        headers: {
            "Content-Type": "application/json",
            ...(options.headers ?? {}),
        },
    });
    
    if (res.status === 204) {
        return undefined as T;
    }

    const data = await res.json().catch(() => null);

    if (!res.ok) {
        const message =
            data?.message ?? data?.title ?? `Ошибка ${res.status}`;
        throw new ApiError(message, res.status);
    }

    return data as T;
}

type QueryValue = string | number | boolean | null | undefined;

function query(params: Record<string, QueryValue>) {
    const search = new URLSearchParams();
    for (const [key, value] of Object.entries(params)) {
        if (value !== undefined && value !== null && value !== "") {
            search.append(key, String(value));
        }
    }
    const value = search.toString();
    return value ? `?${value}` : "";
}

function json(body: unknown): RequestInit {
    return { headers: { "Content-Type": "application/json" }, body: JSON.stringify(body) };
}

export const offeringsApi = {
    getAll: () => apiFetch<OfferingResponse[]>("/api/offerings/"),
    get: (id: string) => apiFetch<OfferingResponse>(`/api/offerings/${id}`),
    create: (request: CreateOfferingRequest) =>
        apiFetch<OfferingResponse>("/api/offerings/", { method: "POST", ...json(request) }),
    change: (id: string, request: ChangeOfferingRequest) =>
        apiFetch<OfferingResponse>(`/api/offerings/${id}${query({ ...request })}`, { method: "PUT" }),
    remove: (id: string) => apiFetch<void>(`/api/offerings/${id}`, { method: "DELETE" }),
};

export const schedulesApi = {
    getFreeTime: (date: string) => apiFetch<FreeTimeResponse>(`/api/schedules/free-time${query({ date })}`),
    getAvailableDates: (offeringIds: string[]) =>
        apiFetch<AvailableDatesResponse>(`/api/schedules/available-dates${query({ offeringIds: offeringIds.join(",") })}`),
    getAll: () => apiFetch<ScheduleResponse[]>("/api/schedules/"),
    create: (request: CreateScheduleRequest) =>
        apiFetch<ScheduleResponse>("/api/schedules/", { method: "POST", ...json(request) }),
    change: (request: ChangeScheduleRequest) =>
        apiFetch<void>("/api/schedules/", { method: "PUT", ...json(request) }),
};

export const appointmentsApi = {
    get: (id: string) => apiFetch<AppointmentDetailResponse>(`/api/appointments/${id}`),
    getAll: () => apiFetch<AdminAppointmentResponse[]>("/api/appointments/all"),
    getByUser: () => apiFetch<UserAppointmentResponse[]>("/api/appointments/user"),
    create: (request: CreateAppointmentRequest) =>
        apiFetch<AppointmentResponse>("/api/appointments/", { method: "POST", ...json(request) }),
    cancel: (id: string) =>
        apiFetch<void>(`/api/appointments/${id}/cancel`, {
            method: "PATCH",
            ...json({ id, status: AppointmentStatus.Canceled } satisfies ChangeAppointmentStatusRequest),
        }),
    changeTime: (request: ChangeAppointmentTimeRequest) =>
        apiFetch<void>(`/api/appointments/${request.id}/change-time`, { method: "PATCH", ...json(request) }),
    changeStatus: (request: ChangeAppointmentStatusRequest) =>
        apiFetch<void>(`/api/appointments/${request.id}/change-status`, { method: "PATCH", ...json(request) }),
    changeOfferingPrice: (appointmentId: string, offeringId: string, price: Money) =>
        apiFetch<void>(`/api/appointments/${appointmentId}/offering/${offeringId}/change-price`, {
            method: "PATCH",
            ...json(price),
        }),
};

export const authApi = {
    sendCode: (request: SendVerificationCodeRequest) =>
        apiFetch<SendVerificationCodeResponse>("/api/auth/send-code", { method: "POST", ...json(request) }),
    login: (request: LoginRequest) =>
        apiFetch<void>("/api/auth/login", { method: "POST", ...json(request) }),
    register: (request: RegisterRequest) =>
        apiFetch<void>("/api/auth/register", { method: "POST", ...json(request) }),
    logout: () => apiFetch<void>("/api/auth/logout", { method: "POST" }),
    me: () => apiFetch<CurrentUser>("/api/auth/me"),
};

export const analyticsApi = {
    getGain: (from: string, to: string) =>
        apiFetch<GainResponse>(`/api/analytics/gain${query({ from, to })}`),
    getOfferingsByPopularity: (from: string, to: string) =>
        apiFetch<OfferingsStatResponse[]>(`/api/analytics/offerings-popularity${query({ from, to })}`),
    getOfferingsByGain: (from: string, to: string) =>
        apiFetch<OfferingsStatResponse[]>(`/api/analytics/offerings-gain${query({ from, to })}`),
    getNoShowRate: (from: string, to: string) =>
        apiFetch<RateResponse>(`/api/analytics/no-show-rate${query({ from, to })}`),
    getAverageCheck: (from: string, to: string) =>
        apiFetch<AverageCheckResponse>(`/api/analytics/average-check${query({ from, to })}`),
    getAverageHourGain: (from: string, to: string) =>
        apiFetch<GainResponse>(`/api/analytics/average-hour-gain${query({ from, to })}`),
    getWorkloadRate: (from: string, to: string) =>
        apiFetch<RateResponse>(`/api/analytics/workload-rate${query({ from, to })}`),
    getBestClients: (limit: number) =>
        apiFetch<BestClientResponse[]>(`/api/analytics/best-clients${query({ limit })}`),
};