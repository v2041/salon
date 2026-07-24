using Domain.Enums;
using Domain.ValueObjects;

namespace Features.Appointments.ConfirmAppointment;

public record ConfirmAppointmentResponse(
    Guid Id,
    DateOnly Date,
    TimeInterval Interval,
    decimal Price,
    AppointmentStatus Status
);