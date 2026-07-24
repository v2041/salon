namespace Features.Users;

public static class UsersModule
{
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users");
        group.MapGet("/{id}", GetUserAppointments.Endpoint.GetUserAppointmentsAsync);
    }
}