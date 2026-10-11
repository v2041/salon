namespace Features.Appointments.ChangeAppointmentTime;

public record ChangeAppointmentTimeRequest(
    Guid Id,
    DateOnly Date,
    TimeOnly Time
);