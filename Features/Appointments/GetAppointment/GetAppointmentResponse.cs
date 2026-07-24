using Domain.ValueObjects;
using Domain.Enums;

namespace Features.Appointments.GetAppointment;

public record GetAppointmentResponse(Guid Id, DateOnly Date, TimeInterval Interval, Guid UserId, AppointmentStatus Status);