namespace Features.Offerings;

public static class OfferingsModule
{
    public static void MapOfferingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/offerings")
            .WithTags("Offerings");
        group.MapGet("/", GetAllOfferings.Endpoint.GetAllOfferingsAsync);
        group.MapGet("/all", GetAllOfferingsAdmin.Endpoint.GetAllOfferingsAdminAsync).RequireAuthorization("Admin");
        group.MapGet("/{id:guid}", GetOffering.Endpoint.GetOfferingAsync);
        group.MapPost("/", CreateOffering.Endpoint.CreateOfferingAsync).RequireAuthorization("Admin");
        group.MapPut("/{id:guid}", ChangeOffering.Endpoint.ChangeOfferingAsync).RequireAuthorization("Admin");
        group.MapDelete("/{id:guid}", DeleteOffering.Endpoint.DeleteOfferingAsync).RequireAuthorization("Admin");
        group.MapPatch("/{id:guid}/restore", RestoreOffering.Endpoint.RestoreOfferingAsync).RequireAuthorization("Admin");
    }
}
