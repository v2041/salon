using Microsoft.AspNetCore.Mvc;

namespace Features.Appointments.GetAllAppointments;

public record GetAllAppointmentsRequest(
    [FromQuery] string Scope,
    [FromQuery] int? Status,
    [FromQuery] int Page = 1,
    [FromQuery] int PageSize = 20
);
