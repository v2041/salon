using Domain.Exceptions;

namespace Domain.ValueObjects;

public record TimeInterval
{
    private TimeInterval(TimeOnly start, TimeOnly end)
    {
        Start = start;
        End = end;
    }

    public TimeOnly Start { get; }
    public TimeOnly End { get; }

    public static TimeInterval Create(TimeOnly start, TimeOnly end)
    {
        if (start >= end)
            throw new BusinessException("Начало должно быть меньше конца");

        return new TimeInterval(start, end);
    }

    public bool IsOverlapping(TimeInterval other) => Start < other.End && End > other.Start;
    public bool IsInside(TimeInterval other) => Start >= other.Start && End <= other.End;
}