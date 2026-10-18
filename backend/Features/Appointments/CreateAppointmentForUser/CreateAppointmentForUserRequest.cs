namespace Features.Appointments.CreateAppointmentForUser;

public record CreateAppointmentForUserRequest(
    string Phone,
    DateOnly Date,
    TimeOnly Time,
    List<Guid> OfferingIds,
    string? FirstName,
    string? LastName
);
