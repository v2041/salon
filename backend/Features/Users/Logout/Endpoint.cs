namespace Features.Users.Logout;

public static class Endpoint
{
    public static IResult Logout(HttpContext context)
    {
        context.Response.Cookies.Delete("some-cookies");
        return Results.NoContent();
    }
}
