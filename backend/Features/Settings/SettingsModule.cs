namespace Features.Settings;

public static class SettingsModule
{
    public static void MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/settings")
            .WithTags("Settings");

        group.MapGet("/", GetSettings.Endpoint.GetSettingsAsync).RequireAuthorization("Admin");
        group.MapPut("/", ChangeSettings.Endpoint.ChangeSettingsAsync).RequireAuthorization("Admin");
        group.MapPost("/apply", ApplySettings.Endpoint.ApplySettingsAsync).RequireAuthorization("Admin");
        group.MapGet("/contacts", GetContacts.Endpoint.GetContactsAsync);
    }
}
