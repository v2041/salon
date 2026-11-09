using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Settings.GetSettings;

public static class Endpoint
{
    public static async Task<IResult> GetSettingsAsync(
        SalonDbContext db,
        CancellationToken token
    )
    {
        var settings = await db.SalonSettings.FirstOrDefaultAsync(token);
        if (settings is null)
            throw new NotFoundException("Настройки не найдены.");

        return Results.Ok(new GetSettingsResponse(
            settings.Address,
            settings.Phone,
            settings.WorkInterval,
            settings.BreakInterval,
            settings.GenerationEnabled,
            settings.HorizonDays,
            settings.WeekendDays));
    }
}
