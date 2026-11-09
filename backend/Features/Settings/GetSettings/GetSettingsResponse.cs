using Domain.ValueObjects;

namespace Features.Settings.GetSettings;

public record GetSettingsResponse(
    string Address,
    string Phone,
    TimeInterval WorkInterval,
    TimeInterval? BreakInterval,
    bool GenerationEnabled,
    int HorizonDays,
    IReadOnlyCollection<DayOfWeek> WeekendDays
);
