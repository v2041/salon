using Domain.Enums;
using Domain.ValueObjects;

namespace Features.Appointments.MissAppointment;

public record MissAppointmentResponse(
    Guid Id,
    DateOnly Date,
    TimeInterval Interval,
    decimal Price,
    AppointmentStatus Status
);