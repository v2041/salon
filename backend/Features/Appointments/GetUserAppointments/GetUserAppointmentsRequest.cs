using Microsoft.AspNetCore.Mvc;

namespace Features.Appointments.GetUserAppointments;

public record GetUserAppointmentsRequest(
    [FromQuery] string Scope,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 20
);
