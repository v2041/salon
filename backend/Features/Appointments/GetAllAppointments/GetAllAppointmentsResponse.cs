using Domain.Enums;
using Domain.ValueObjects;

namespace Features.Appointments.GetAllAppointments;

public record AdminAppointmentOfferingResponse(
    Guid OfferingId,
    string Title,
    Money Price,
    TimeSpan Duration
);

public record GetAllAppointmentsResponse(
    Guid Id,
    DateOnly Date,
    TimeInterval Interval,
    Money Price,
    IEnumerable<AdminAppointmentOfferingResponse> Offerings,
    Guid UserId,
    string UserName,
    string UserPhone,
    AppointmentStatus Status
);
