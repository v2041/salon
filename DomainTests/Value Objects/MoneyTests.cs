using Domain.Exceptions;
using Domain.ValueObjects;

namespace DomainTests.Value_Objects;

public class MoneyTests
{
    public class FromDecimal
    {
        [Fact]
        public void FromDecimal_WithValidValue_ReturnsMoney()
        {
            decimal validValue = 100;
            var money = Money.FromDecimal(validValue);
            Assert.Equal(validValue, money.Value);
        }

        [Fact]
        public void FromDecimal_WithNegativeValue_ThrowsBusinessException()
        {
            Assert.Throws<BusinessException>(() => Money.FromDecimal(-100));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void FromDecimal_WithNullOrEmptyOrWhiteSpaceCurrency_ThrowsBusinessException(
            string emptyOrNullOrWhiteSpaceCurrency)
        {
            Assert.Throws<BusinessException>(() => Money.FromDecimal(100, emptyOrNullOrWhiteSpaceCurrency));
        }
        
        [Fact]
        public void FromDecimal_WithLowerCaseCurrency_ConvertsToUpperCase()
        {
            var money = Money.FromDecimal(100, "rub");
            Assert.Equal("RUB", money.Currency);
        }
    }
}