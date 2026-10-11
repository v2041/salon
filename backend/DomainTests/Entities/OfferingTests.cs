using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace DomainTests.Entities;

public class OfferingTests
{
    public class Create
    {
        private readonly Offering _offering = Offering.Create(
            Money.FromDecimal(500),
            "услуга",
            "описание услуги",
            TimeSpan.FromMinutes(30),
            Category.Hair
        );

        [Fact]
        public void Create_WithValid_ReturnsOffering()
        {
            var offering = Offering.Create(
                Money.FromDecimal(500),
                "title",
                "desc",
                TimeSpan.FromMinutes(30),
                Category.Hair
            );
            Assert.IsType<Offering>(offering);
        }

        [Theory]
        [InlineData("", "description", 30)]
        [InlineData(" ", "description", 30)]
        [InlineData(null, "description", 30)]
        [InlineData("title", "", 30)]
        [InlineData(" ", " ", 30)]
        [InlineData("title", null, 30)]
        [InlineData("title", "description", 0)]
        public void Create_WithInvalid_ThrowsBusinessException(
            string title,
            string description,
            int durationMinutes
        )
        {
            Assert.Throws<BusinessException>(() =>
                Offering.Create(Money.FromDecimal(100), title, description, TimeSpan.FromMinutes(durationMinutes),
                    Category.Hair));
        }
    }

    public class ChangeDuration
    {
        private readonly Offering _offering = Offering.Create(
            Money.FromDecimal(500),
            "услуга",
            "описание услуги",
            TimeSpan.FromMinutes(30),
            Category.Hair
        );

        [Fact]
        public void ChangeDuration_WithValidDuration_UpdatesDuration()
        {
            var validDuration = TimeSpan.FromMinutes(30);
            _offering.ChangeDuration(validDuration);
            Assert.Equal(validDuration, _offering.Duration);
        }

        [Fact]
        public void ChangeDuration_WithSmallDuration_ThrowsBusinessException()
        {
            Assert.Throws<BusinessException>(() => _offering.ChangeDuration(TimeSpan.FromMinutes(1)));
        }
    }

    public class ChangeTitle
    {
        private readonly Offering _offering = Offering.Create(
            Money.FromDecimal(500),
            "услуга",
            "описание услуги",
            TimeSpan.FromMinutes(30),
            Category.Hair
        );

        [Fact]
        public void ChangeTitle_WithTooBigTitle_ThrowsBusinessException()
        {
            var bigTitle = new string('t', Offering.MAX_TITLE_LENGTH + 1);
            Assert.Throws<BusinessException>(() => _offering.ChangeTitle(bigTitle));
        }

        [Fact]
        public void ChangeTitle_WithValidTitle_UpdatesTitle()
        {
            var validTitle = new string('t', Offering.MAX_TITLE_LENGTH - 1);
            _offering.ChangeTitle(validTitle);
            Assert.Equal(validTitle, _offering.Title);
        }

        [Fact]
        public void ChangeTitle_WithMaxTitle_UpdatesTitle()
        {
            var maxTitle = new string('t', Offering.MAX_TITLE_LENGTH);
            _offering.ChangeTitle(maxTitle);
            Assert.Equal(maxTitle, _offering.Title);
        }

        [Fact]
        public void ChangeTitle_WithEmptyTitle_ThrowsBusinessException()
        {
            var emptyTitle = "";
            Assert.Throws<BusinessException>(() => _offering.ChangeTitle(emptyTitle));
        }
    }

    public class ChangeDescription
    {
        private readonly Offering _offering = Offering.Create(
            Money.FromDecimal(500),
            "услуга",
            "описание услуги",
            TimeSpan.FromMinutes(30),
            Category.Hair
        );

        [Fact]
        public void ChangeDescription_WithValidDescription_UpdatesDescription()
        {
            const string validDescription = "описание";
            _offering.ChangeDescription(validDescription);
            Assert.Equal(validDescription, _offering.Description);
        }
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ChangeDescription_WithNullOrEmptyOrWhiteSpaceDescription_ThrowsBusinessException(string description)
        {
            Assert.Throws<BusinessException>(() => _offering.ChangeDescription(description));
        }
    }

    public class ChangePrice
    {
        private readonly Offering _offering = Offering.Create(
            Money.FromDecimal(500),
            "услуга",
            "описание услуги",
            TimeSpan.FromMinutes(30),
            Category.Hair
        );

        [Fact]
        public void ChangePrice_WithValidPrice_UpdatesPrice()
        {
            var validPrice = Money.FromDecimal(100);
            _offering.ChangePrice(validPrice);
            Assert.Equal(validPrice, _offering.Price);
        }
    }

    public class ChangeCategory
    {
        private readonly Offering _offering = Offering.Create(
            Money.FromDecimal(500),
            "услуга",
            "описание услуги",
            TimeSpan.FromMinutes(30),
            Category.Hair
        );

        [Fact]
        public void ChangeCategory_WithValidCategory_UpdatesCategory()
        {
            _offering.ChangeCategory(Category.Nails);
            Assert.Equal(Category.Nails, _offering.Category);
        }
    }
}
