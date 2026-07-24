using Domain.Enums;
using Domain.ValueObjects;

namespace Features.Appointments.GetAllAppointments;

public record GetAllAppointmentsResponse(Guid Id, DateOnly Date, TimeInterval Interval, Guid UserId, AppointmentStatus Status);