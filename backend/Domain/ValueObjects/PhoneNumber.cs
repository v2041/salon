using Domain.Exceptions;

namespace Domain.ValueObjects;

public record PhoneNumber
{
    public string Value { get; init; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        var normalized = NormalizePhone(value);
        return new PhoneNumber(normalized);
    }
    
    public static string NormalizePhone(string? rawPhone)
    {
        if (string.IsNullOrWhiteSpace(rawPhone))
            throw new BusinessException("Неверный формат номера телефона");

        Span<char> digits = stackalloc char[11];
        var count = 0;

        foreach (var c in rawPhone.AsSpan())
        {
            if (!char.IsAsciiDigit(c))
                continue;

            if (count == 11)
                throw new BusinessException("Неверный формат номера телефона");

            digits[count++] = c;
        }

        return count switch
        {
            11 when digits[0] == '8' => $"+7{digits[1..]}",
            11 when digits[0] == '7' => $"+{digits[..11]}",
            
            10 when digits[0] == '7' => $"+{digits[..10]}",
            10 => $"+7{digits[..10]}",
            
            _ => throw new BusinessException("Неверный формат номера телефона")
        };
    }
}