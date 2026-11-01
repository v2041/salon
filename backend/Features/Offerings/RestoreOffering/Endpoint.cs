using Domain.Exceptions;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Features.Offerings.RestoreOffering;

public static class Endpoint
{
    public static async Task<IResult> RestoreOfferingAsync(
        [AsParameters] RestoreOfferingRequest request,
        SalonDbContext db,
        CancellationToken token
    )
    {
        var offering = await db.Offerings.FirstOrDefaultAsync(o => o.Id == request.Id, token);
        if (offering == null)
            throw new NotFoundException("Услуга не найдена");

        offering.Restore();
        await db.SaveChangesAsync(token);
        return Results.NoContent();
    }
}

public record RestoreOfferingRequest(Guid Id);
