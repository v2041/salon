using Domain.Enums;
using Domain.ValueObjects;

namespace Features.Schedules.GetAllSchedules;

public record ScheduleAppointmentResponse(
    Guid Id,
    TimeInterval Interval,
    AppointmentStatus Status,
    Guid UserId,
    string UserName,
    Money Price
);

public record GetScheduleResponse(
    Guid Id,
    DateOnly Date,
    bool IsWorking,
    TimeInterval WorkInterval,
    TimeInterval? BreakInterval,
    IEnumerable<ScheduleAppointmentResponse> Appointments
);
