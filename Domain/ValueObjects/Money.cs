using System.Text.Json.Serialization;
using Domain.Exceptions;

namespace Domain.ValueObjects;

public record Money
{
    public decimal Value { get; init; }
    public string Currency { get; init; }

    [JsonConstructor]
    private Money(decimal value, string currency)
    {
        if (value < 0)
            throw new BusinessException("Цена не должна быть отрицательной");
        if (string.IsNullOrWhiteSpace(currency))
            throw new BusinessException("Валюта должна быть указана");
        Value = value;
        Currency = currency.ToUpperInvariant();
    }

    public static Money FromDecimal(decimal value, string currency = "RUB")
    {
        if (value < 0)
            throw new BusinessException("Цена не должна быть отрицательной");
        if (string.IsNullOrWhiteSpace(currency))
            throw new BusinessException("Валюта должна быть указана");
        return new Money(value, currency);
    }

    public override string ToString() => $"{Value} {Currency}";
}