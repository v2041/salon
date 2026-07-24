namespace Features.Appointments;

public static class AppointmentsModule
{
    public static void MapAppointmentsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/appointments")
            .WithTags("Appointments");
        group.MapGet("/{id}", GetAppointment.Endpoint.GetAppointmentAsync);
        group.MapGet("/", GetAllAppointments.Endpoint.GetAllAppointmentsAsync);
        group.MapPost("/", CreateAppointment.Endpoint.CreateAppointmentAsync);
        group.MapPatch("/{id}/change", ChangeAppointmentPrice.Endpoint.ChangeAppointmentPriceAsync);
        group.MapPatch("/{id}/cancel", CancelAppointment.Endpoint.CancelAppointmentAsync);
        group.MapPatch("/{id}/reject", RejectAppointment.Endpoint.RejectAppointmentAsync);
        group.MapPatch("/{id}/confirm", ConfirmAppointment.Endpoint.ConfirmAppointmentAsync);
        group.MapPatch("/{id}/miss", MissAppointment.Endpoint.MissAppointmentAsync);
    }
}