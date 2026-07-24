namespace Features.Appointments.ChangeAppointmentPrice;

public record ChangeAppointmentPriceRequest(Guid Id, decimal Price);