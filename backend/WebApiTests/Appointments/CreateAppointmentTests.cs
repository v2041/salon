using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using WebApiTests.Infrastructure;

namespace WebApiTests.Appointments;

[Collection(WebApiCollection.Name)]
public class CreateAppointmentTests(WebApiFixture fixture) : HttpTestBase(fixture)
{
    private static string Tomorrow => DateOnly.FromDateTime(DateTime.Now).AddDays(1).ToString("yyyy-MM-dd");

    [Fact]
    public async Task Create_WithInactiveOffering_ReturnsProblemJson400()
    {
        var user = await SeedUserAsync();
        await SeedScheduleAsync(DateOnly.FromDateTime(DateTime.Now).AddDays(1));
        var offering = await SeedOfferingAsync("Снятая услуга", isActive: false);
        AuthenticateAs(user);

        var response = await Client.PostAsJsonAsync("/api/appointments/", new
        {
            date = Tomorrow,
            time = "11:00:00",
            offeringIds = new[] { offering.Id }
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Create_WithUnknownOffering_Returns404()
    {
        var user = await SeedUserAsync();
        await SeedScheduleAsync(DateOnly.FromDateTime(DateTime.Now).AddDays(1));
        AuthenticateAs(user);

        var response = await Client.PostAsJsonAsync("/api/appointments/", new
        {
            date = Tomorrow,
            time = "11:00:00",
            offeringIds = new[] { Guid.NewGuid() }
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_TwoParallelRequestsForSameSlot_OneSucceedsAndOneConflicts()
    {
        var user = await SeedUserAsync();
        await SeedScheduleAsync(DateOnly.FromDateTime(DateTime.Now).AddDays(1));
        var offering = await SeedOfferingAsync("Стрижка");
        AuthenticateAs(user);

        var body = new
        {
            date = Tomorrow,
            time = "12:00:00",
            offeringIds = new[] { offering.Id }
        };

        var responses = await Task.WhenAll(
            Client.PostAsJsonAsync("/api/appointments/", body),
            Client.PostAsJsonAsync("/api/appointments/", body));

        var statuses = responses.Select(r => r.StatusCode).ToList();
        Assert.Contains(HttpStatusCode.Created, statuses);
        Assert.Contains(HttpStatusCode.BadRequest, statuses);

        var conflict = responses.Single(r => r.StatusCode == HttpStatusCode.BadRequest);
        Assert.Equal("application/problem+json", conflict.Content.Headers.ContentType?.MediaType);

        var problem = JsonDocument.Parse(await conflict.Content.ReadAsStringAsync());
        Assert.Contains("занято", problem.RootElement.GetProperty("detail").GetString());
    }
}
