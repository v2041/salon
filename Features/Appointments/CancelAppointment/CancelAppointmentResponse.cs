using Domain.Enums;
using Domain.ValueObjects;

namespace Features.Appointments.CancelAppointment;

public record CancelAppointmentResponse(
    Guid Id,
    DateOnly Date,
    TimeInterval Interval,
    decimal Price,
    AppointmentStatus Status
);