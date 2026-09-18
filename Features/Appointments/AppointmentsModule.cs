using Endpoint = Features.Appointments.ChangeAppointmentStatus.Endpoint;

namespace Features.Appointments;

public static class AppointmentsModule
{
    public static void MapAppointmentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments")
            .WithTags("Appointments");
        group.MapGet("/{id}", GetAppointment.Endpoint.GetAppointmentAsync);
        group.MapGet("/all", GetAllAppointments.Endpoint.GetAllAppointmentsAsync);
        group.MapGet("/user", GetUserAppointments.Endpoint.GetUserAppointmentsAsync);
        group.MapPost("/", CreateAppointment.Endpoint.CreateAppointmentAsync);
        group.MapPatch("{id}/status", ChangeAppointmentStatus.Endpoint.ChangeAppointmentStatusAsync);
    }
}