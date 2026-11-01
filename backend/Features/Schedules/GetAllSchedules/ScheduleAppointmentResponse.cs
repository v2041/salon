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
