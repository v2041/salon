using Domain.Exceptions;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Features.Settings.ApplySettings;

public static class Endpoint
{
    public static async Task<IResult> ApplySettingsAsync(
        SalonDbContext db,
        ScheduleGenerationRunner runner,
        CancellationToken token
    )
    {
        var settings = await db.SalonSettings.FirstOrDefaultAsync(token);
        if (settings is null)
            throw new NotFoundException("Настройки не найдены.");

        var affectedDays = await runner.ApplyAsync(settings, token);
        return Results.Ok(new ApplySettingsResponse(affectedDays));
    }
}
