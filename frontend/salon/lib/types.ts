export enum AppointmentStatus {
    Pending = 0,
    Confirmed = 1,
    Completed = 2,
    Canceled = 3,
    Missed = 4,
    Rejected = 5,
}

export enum Category {
    Hair = 0,
    Nails = 1,
    Face = 2,
    Other = 3,
}

export enum UserRole {
    Default = 0,
    Admin = 1,
}

export type Money = {
    value: number;
    currency: string;
};

export type TimeInterval = {
    start: string;
    end: string;
};

export type Offering = {
    id: string;
    title: string;
    description: string;
    price: Money;
    duration: string;
    category: Category;
    isActive: boolean;
};

export type AppointmentOffering = {
    appointmentId: string;
    offeringId: string;
    price: Money;
    duration: string;
};

export type Schedule = {
    id: string;
    date: string;
    isWorking: boolean;
    workInterval: TimeInterval;
    breakInterval: TimeInterval | null;
};

export type Appointment = {
    id: string;
    scheduleId: string;
    userId: string;
    interval: TimeInterval;
    status: AppointmentStatus;
    price: Money;
    offerings: AppointmentOffering[];
};

export type User = {
    id: string;
    firstName: string;
    lastName: string | null;
    phone: string;
    role: UserRole;
    createdAt: string;
    lastLoginAt: string;
};

export type OfferingResponse = Omit<Offering, "isActive">;

export type ScheduleResponse = Omit<Schedule, "isWorking">;

export type AppointmentResponse = {
    id: string;
    date: string;
    interval: TimeInterval;
    status: AppointmentStatus;
    price: Money;
};

export type AppointmentDetailResponse = AppointmentResponse & {
    userId: string;
};

export type AdminAppointmentResponse = {
    id: string;
    date: string;
    interval: TimeInterval;
    price: Money;
    offeringsIds: string[];
    userId: string;
    status: AppointmentStatus;
};

export type FreeTimeResponse = {
    freeIntervals: TimeInterval[];
};

export type AvailableDatesResponse = {
    dates: string[];
};

export type CreateOfferingRequest = {
    price: number;
    title: string;
    description: string;
    duration: string;
    category: Category;
};

export type ChangeOfferingRequest = {
    price?: number | null;
    title?: string | null;
    description?: string | null;
    duration?: string | null;
    category?: Category | null;
};

export type CreateScheduleRequest = {
    date: string;
    workInterval: TimeInterval;
    breakInterval: TimeInterval | null;
};

export type ChangeScheduleRequest = {
    id: string;
    isWorking: boolean;
    workInterval: TimeInterval;
    breakInterval: TimeInterval | null;
};

export type CreateAppointmentRequest = {
    date: string;
    time: string;
    offeringIds: string[];
};

export type ChangeAppointmentTimeRequest = {
    id: string;
    date: string;
    time: string;
};

export type ChangeAppointmentStatusRequest = {
    id: string;
    status: AppointmentStatus;
};

export type SendVerificationCodeRequest = {
    phone: string;
};

export type SendVerificationCodeResponse = {
    expiresAt: string;
};

export type LoginRequest = {
    code: string;
    phone: string;
};

export type RegisterRequest = {
    code: string;
    phone: string;
    firstName: string;
    lastName: string;
};

export type GainResponse = {
    gain: Money;
};

export type AverageCheckResponse = {
    price: Money;
};

export type OfferingsStatResponse = {
    name: string;
    count: number;
    price: Money;
};

export type RateResponse = {
    rate: number;
};

export type BestClientResponse = {
    name: string;
    phoneNumber: string;
    spent: Money;
};
