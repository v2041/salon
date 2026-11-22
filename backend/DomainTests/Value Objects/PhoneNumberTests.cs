using Domain.Exceptions;
using Domain.ValueObjects;

namespace DomainTests.Value_Objects;

public class PhoneNumberTests
{
    public class NormalizePhoneNumber
    {
        [Fact]
        public void NormalizePhoneNumber_WithNormalized_ReturnsNormalized()
        {
            var value = "+79991234567";
            var normalized = PhoneNumber.NormalizePhone(value);
            Assert.Equal(value, normalized);
        }

        [Fact]
        public void NormalizePhoneNumber_WithRawValid_ReturnsNormalized()
        {
            var value = "+7 (999) 123-45-67";
            var normalized = PhoneNumber.NormalizePhone(value);
            Assert.Equal("+79991234567", normalized);
        }

        [Theory]
        [InlineData("+19991234567")]
        [InlineData("991234567")]
        [InlineData("+79991221323134567")]
        [InlineData("123")]
        public void NormalizePhoneNumber_WithInvalid_ReturnsBusinessException(string invalidNumber)
        {
            Assert.Throws<BusinessException>(() => PhoneNumber.NormalizePhone(invalidNumber));
        }
    }
}