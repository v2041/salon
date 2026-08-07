namespace Features.Schedules;

public static class SchedulesModule
{
    public static void MapSchedulesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/schedules")
            .WithTags("Schedules");
        // group.MapGet("/{id}", GetOffering.Endpoint.GetOfferingAsync);
        group.MapGet("/free-time", GetFreeTime.Endpoint.GetFreeTimeAsync);
        group.MapGet("/available-dates", GetAvailableDates.Endpoint.GetAvailableDatesAsync);
        group.MapGet("/", GetAllSchedules.Endpoint.GetAllSchedulesAsync);
        group.MapPost("/", CreateSchedule.Endpoint.CreateScheduleAsync);
        group.MapPut("/", ChangeSchedule.Endpoint.ChangeScheduleAsync);
    }
}