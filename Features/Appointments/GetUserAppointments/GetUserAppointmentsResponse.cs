using Domain.Enums;
using Domain.ValueObjects;

namespace Features.Appointments.GetUserAppointments;

public record GetUserAppointmentsResponse(
    Guid Id,
    DateOnly Date,
    TimeInterval Interval,
    Guid UserId,
    AppointmentStatus Status,
    Money Price
);