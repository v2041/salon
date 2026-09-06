using Domain.ValueObjects;

namespace Features.Appointments.ChangeAppointmentOffering;

public record ChangeAppointmentOfferingPriceRequest(
    Guid AppointmentId,
    Guid OfferingId,
    Money Price
);