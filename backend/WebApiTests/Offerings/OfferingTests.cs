using System.Net;
using System.Net.Http.Json;
using Domain.Enums;
using WebApiTests.Infrastructure;

namespace WebApiTests.Offerings;

[Collection(WebApiCollection.Name)]
public class OfferingTests(WebApiFixture fixture) : HttpTestBase(fixture)
{
    [Fact]
    public async Task AvailableDates_WithTwoOfferingsOfSameDuration_ReturnsDates()
    {
        var date = DateOnly.FromDateTime(DateTime.Now).AddDays(1);
        await SeedScheduleAsync(date);
        var first = await SeedOfferingAsync("Стрижка", minutes: 60);
        var second = await SeedOfferingAsync("Укладка", minutes: 60);

        var response = await Client.GetAsync(
            $"/api/schedules/available-dates?offeringIds={first.Id},{second.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AvailableDatesResponse>();
        Assert.Contains(date.ToString("yyyy-MM-dd"), body!.Dates);
    }

    [Fact]
    public async Task AvailableDates_WithInactiveOffering_Returns404()
    {
        await SeedScheduleAsync(DateOnly.FromDateTime(DateTime.Now).AddDays(1));
        var inactive = await SeedOfferingAsync("Снятая", isActive: false);

        var response = await Client.GetAsync(
            $"/api/schedules/available-dates?offeringIds={inactive.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PublicList_ExcludesInactiveOfferings()
    {
        await SeedOfferingAsync("Активная");
        await SeedOfferingAsync("Снятая", isActive: false);

        var offerings = await Client.GetFromJsonAsync<List<OfferingDto>>("/api/offerings/");

        Assert.Single(offerings!);
        Assert.Equal("Активная", offerings![0].Title);
    }

    [Fact]
    public async Task PublicGet_WithInactiveOffering_Returns404()
    {
        var inactive = await SeedOfferingAsync("Снятая", isActive: false);

        var response = await Client.GetAsync($"/api/offerings/{inactive.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Restore_InactiveOffering_BringsItBackToPublicList()
    {
        var admin = await SeedUserAsync(role: UserRole.Admin);
        var inactive = await SeedOfferingAsync("Снятая", isActive: false);
        AuthenticateAs(admin);

        var restore = await Client.PatchAsync($"/api/offerings/{inactive.Id}/restore", null);
        Assert.Equal(HttpStatusCode.NoContent, restore.StatusCode);

        var response = await Client.GetAsync($"/api/offerings/{inactive.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed record OfferingDto(Guid Id, string Title);
    private sealed record AvailableDatesResponse(List<string> Dates);
}
