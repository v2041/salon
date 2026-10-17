using Microsoft.AspNetCore.Mvc;

namespace Features.Appointments.ChangeAppointmentOfferingDuration;

public record ChangeAppointmentOfferingDurationRequest(
    [FromRoute] Guid AppointmentId,
    [FromRoute] Guid OfferingId,
    [FromBody] TimeSpan Duration
);
