using Domain.ValueObjects;

namespace Features.Settings.ChangeSettings;

public record ChangeSettingsRequest(
    string Address,
    string Phone,
    TimeInterval WorkInterval,
    TimeInterval? BreakInterval,
    bool GenerationEnabled,
    int HorizonDays,
    IReadOnlyCollection<DayOfWeek> WeekendDays
);
