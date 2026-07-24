using Domain.Enums;
using Domain.ValueObjects;

namespace Features.Appointments.RejectAppointment;

public record RejectAppointmentResponse(
    Guid Id,
    DateOnly Date,
    TimeInterval Interval,
    decimal Price,
    AppointmentStatus Status
);