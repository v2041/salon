using Domain.Enums;
using Domain.ValueObjects;

namespace Features.Appointments.CreateAppointment;

public record CreateAppointmentResponse(
    Guid Id,
    DateOnly Date,
    TimeInterval Interval,
    AppointmentStatus Status,
    Money Price
);