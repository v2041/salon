using System.Text.Json.Serialization;
using Domain.Exceptions;

namespace Domain.ValueObjects;

public record TimeInterval
{
    private TimeInterval()
    {
    }

    [JsonConstructor]
    private TimeInterval(TimeOnly start, TimeOnly end)
    {
        if (start >= end)
            throw new BusinessException("Начало должно быть меньше конца");
        Start = start;
        End = end;
    }

    public TimeOnly Start { get; }
    public TimeOnly End { get; }

    public static TimeInterval Create(TimeOnly start, TimeOnly end)
    {
        return new TimeInterval(start, end);
    }

    public static TimeInterval Create(TimeOnly start, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero || duration >= TimeSpan.FromDays(1))
            throw new BusinessException("Интервал не должен выходить за пределы суток.");
        if (start.Ticks + duration.Ticks >= TimeSpan.TicksPerDay)
            throw new BusinessException("Интервал не должен выходить за пределы суток.");
        return new TimeInterval(start, new TimeOnly(start.Ticks + duration.Ticks));
    }

    public static TimeSpan IntervalsDuration(IEnumerable<TimeInterval> intervals)
        => intervals.Aggregate(TimeSpan.Zero, (current, interval) => current + interval.Duration);


    public bool IsOverlapping(TimeInterval other) => Start < other.End && End > other.Start;
    public bool IsInside(TimeInterval other) => Start >= other.Start && End <= other.End;
    public TimeSpan Duration => End - Start;
}