using Domain.Enums;

namespace Features.Appointments.ChangeAppointmentStatus;

public record ChangeAppointmentStatusRequest(Guid Id, AppointmentStatus Status);