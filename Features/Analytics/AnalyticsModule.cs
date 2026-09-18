namespace Features.Analytics;

public static class AnalyticsModule
{
    public static void MapAnalyticsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("api/analytics/").WithTags("Analytics");
        group.MapGet("/gain", GetGain.Endpoint.GetGainAsync);
        group.MapGet("/offerings-popularity", GetOfferingsByPopularity.Endpoint.GetOfferingsByPopularityAsync);
    }
}