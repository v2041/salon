using Endpoint = Features.Appointments.ChangeAppointmentStatus.Endpoint;

namespace Features.Appointments;

public static class AppointmentsModule
{
    public static void MapAppointmentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/appointments")
            .WithTags("Appointments")
            .RequireAuthorization("User");
        group.MapGet("/{id}", GetAppointment.Endpoint.GetAppointmentAsync);
        group.MapGet("/all", GetAllAppointments.Endpoint.GetAllAppointmentsAsync).RequireAuthorization("Admin");
        group.MapGet("/user", GetUserAppointments.Endpoint.GetUserAppointmentsAsync);
        group.MapPost("/", CreateAppointment.Endpoint.CreateAppointmentAsync);
        group.MapPatch("{id}/cancel", CancelAppointment.Endpoint.CancelAppointmentAsync);
        group.MapPatch("{id}/change-status", ChangeAppointmentStatus.Endpoint.ChangeAppointmentStatusAsync).RequireAuthorization("Admin");
        group.MapPatch("{AppointmentId}/offering/{OfferingId}/change-price", ChangeAppointmentOfferingPrice.Endpoint.ChangeAppointmentOfferingPriceAsync).RequireAuthorization("Admin");
    }
}