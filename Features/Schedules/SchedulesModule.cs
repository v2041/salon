namespace Features.Schedules;

public static class SchedulesModule
{
    public static void MapSchedulesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/schedules")
            .WithTags("Schedules");

        group.MapGet("/free-time", GetFreeTime.Endpoint.GetFreeTimeAsync);
        group.MapGet("/available-dates", GetAvailableDates.Endpoint.GetAvailableDatesAsync);
        group.MapGet("/", GetAllSchedules.Endpoint.GetAllSchedulesAsync).RequireAuthorization("Admin");
        group.MapPost("/", CreateSchedule.Endpoint.CreateScheduleAsync).RequireAuthorization("Admin");
        group.MapPut("/", ChangeSchedule.Endpoint.ChangeScheduleAsync).RequireAuthorization("Admin");
    }
}