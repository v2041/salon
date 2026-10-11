namespace Features.Appointments.CreateAppointment;

public record CreateAppointmentRequest(
    DateOnly Date,
    TimeOnly Time,
    List<Guid> OfferingIds
);