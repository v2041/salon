namespace Features.Appointments.CreateAppointment;
public record CreateAppointmentRequest(DateOnly Date, TimeOnly Time, Guid UserId, List<Guid> OfferingIds);