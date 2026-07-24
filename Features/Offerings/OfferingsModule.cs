namespace Features.Offerings;

public static class OfferingsModule
{
    public static void MapOfferingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/offerings")
            .WithTags("Offerings");
        group.MapGet("/{id}", GetOffering.Endpoint.GetOfferingAsync);
        group.MapGet("/", GetAllOfferings.Endpoint.GetAllOfferingsAsync);
        group.MapPost("/", CreateOffering.Endpoint.CreateOfferingAsync);
        group.MapDelete("/{id}", DeleteOffering.Endpoint.DeleteOfferingAsync);
    }
}