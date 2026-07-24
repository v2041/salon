using Domain.Enums;
using Domain.ValueObjects;

namespace Features.Users.GetUserAppointments;

public record GetUserAppointmentsResponse(Guid Id, DateOnly Date, TimeInterval Interval, Guid UserId, AppointmentStatus Status);