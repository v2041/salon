using Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Features.Appointments.ChangeAppointmentOfferingPrice;

public record ChangeAppointmentOfferingPriceRequest(
    [FromRoute] Guid AppointmentId,
    [FromRoute] Guid OfferingId,
    [FromBody] Money Price
);