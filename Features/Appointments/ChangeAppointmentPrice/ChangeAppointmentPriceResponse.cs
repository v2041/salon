using Domain.Enums;
using Domain.ValueObjects;

namespace Features.Appointments.ChangeAppointmentPrice;

public record ChangeAppointmentPriceResponse(
    Guid Id,
    DateOnly Date,
    TimeInterval Interval,
    decimal Price,
    AppointmentStatus Status
);