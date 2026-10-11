using Domain.Enums;
using Domain.ValueObjects;

namespace Features.Appointments.GetAllAppointments;

public record GetAllAppointmentsResponse(
    Guid Id,
    DateOnly Date,
    TimeInterval Interval,
    Money Price,
    IEnumerable<Guid> OfferingsIds,
    Guid UserId,
    AppointmentStatus Status
);
