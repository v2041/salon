using Domain.ValueObjects;

namespace Features.Analytics.AverageHourGain;

public record GetAverageHourGainResponse(
    Money Gain
);