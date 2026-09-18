namespace Infrastructure.Extensions;

public static class AuthorizationExtension
{
    public static void RequireAdmin(this IEndpointRouteBuilder builder)
    {
        builder.RequireAdmin();
    }
}