namespace Features.Users;

public static class UsersModule
{
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/auth/")
            .WithTags("Users");
        group.MapPost("/vk",VkLogin.Endpoint.VkLoginAsync)
            .Produces<VkLogin.VkLoginResponse>();
        group.MapPost("/logout",Logout.Endpoint.Logout);
        group.MapGet("/me",GetCurrentUser.Endpoint.GetCurrentUserAsync).RequireAuthorization();
        group.MapPatch("/me",UpdateName.Endpoint.UpdateNameAsync).RequireAuthorization().Produces(204);
    }
}
