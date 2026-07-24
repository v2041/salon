using Features.Appointments.ConfirmAppointment;
using Features.Appointments.MissAppointment;
using FluentValidation;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Features.Appointments.RejectAppointment;

public class Validator : AbstractValidator<RejectAppointmentRequest>
{
    private readonly SalonDbContext _db;

    public Validator(SalonDbContext db)
    {
        _db = db;

        RuleFor(request => request.Id)
            .MustAsync(async (id, token) =>
                await _db.Appointments.AnyAsync(a =>  a.Id == id, token)
            )
            .WithMessage("Запись не найдена");
    }
}