using Domain.Enums;
using Domain.ValueObjects;

namespace Features.Appointments.ChangeAppointment;

public record ChangeAppointmentTimeRequest(
    Guid Id,
    DateOnly? Date,
    TimeOnly? Time
);