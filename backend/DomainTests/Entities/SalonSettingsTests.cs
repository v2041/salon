using Domain.Entities;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace DomainTests.Entities;

public class SalonSettingsTests
{
    private const string Address = "ул. Тверская, 1";
    private const string Phone = "+79991234567";

    private static readonly TimeInterval WorkInterval =
        TimeInterval.Create(new TimeOnly(10, 0, 0), new TimeOnly(18, 0, 0));

    private static readonly TimeInterval BreakInterval =
        TimeInterval.Create(new TimeOnly(15, 0, 0), new TimeOnly(15, 20, 0));

    private static SalonSettings CreateValid() =>
        SalonSettings.Create(Address, Phone, WorkInterval, BreakInterval, true, 14, [DayOfWeek.Sunday]);

    public class Create
    {
        [Fact]
        public void Create_WithValid_ReturnsSettings()
        {
            var settings = CreateValid();

            Assert.Equal(Address, settings.Address);
            Assert.Equal(Phone, settings.Phone);
            Assert.Equal(WorkInterval, settings.WorkInterval);
            Assert.Equal(BreakInterval, settings.BreakInterval);
            Assert.True(settings.GenerationEnabled);
            Assert.Equal(14, settings.HorizonDays);
            Assert.Equal(new[] { DayOfWeek.Sunday }, settings.WeekendDays);
        }

        [Fact]
        public void Create_WithoutBreak_ReturnsSettings()
        {
            var settings = SalonSettings.Create(Address, Phone, WorkInterval, null, true, 14, []);
            Assert.Null(settings.BreakInterval);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithEmptyAddress_ThrowsBusinessException(string address)
            => Assert.Throws<BusinessException>(() =>
                SalonSettings.Create(address, Phone, WorkInterval, BreakInterval, true, 14, []));

        [Fact]
        public void Create_WhenAddressTooLong_ThrowsBusinessException()
        {
            var address = new string('а', SalonSettings.MAX_ADDRESS_LENGTH + 1);
            Assert.Throws<BusinessException>(() =>
                SalonSettings.Create(address, Phone, WorkInterval, BreakInterval, true, 14, []));
        }

        [Theory]
        [InlineData("89991234567")]
        [InlineData("+7999123456")]
        [InlineData("+799912345678")]
        [InlineData("+89991234567")]
        public void Create_WithInvalidPhone_ThrowsBusinessException(string phone)
            => Assert.Throws<BusinessException>(() =>
                SalonSettings.Create(Address, phone, WorkInterval, BreakInterval, true, 14, []));

        [Fact]
        public void Create_WhenWorkIntervalTooShort_ThrowsBusinessException()
        {
            var workInterval = TimeInterval.Create(new TimeOnly(10, 0, 0), new TimeOnly(11, 59, 0));
            Assert.Throws<BusinessException>(() =>
                SalonSettings.Create(Address, Phone, workInterval, null, true, 14, []));
        }

        [Fact]
        public void Create_WhenBreakNotInsideWork_ThrowsBusinessException()
        {
            var breakInterval = TimeInterval.Create(new TimeOnly(19, 0, 0), new TimeOnly(19, 30, 0));
            Assert.Throws<BusinessException>(() =>
                SalonSettings.Create(Address, Phone, WorkInterval, breakInterval, true, 14, []));
        }

        [Theory]
        [InlineData(10, 15)]
        [InlineData(16, 18)]
        public void Create_WhenBreakTouchesWorkBorder_ThrowsBusinessException(int startHour, int endHour)
        {
            var breakInterval = TimeInterval.Create(
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(startHour)),
                TimeOnly.FromTimeSpan(TimeSpan.FromHours(endHour)));
            Assert.Throws<BusinessException>(() =>
                SalonSettings.Create(Address, Phone, WorkInterval, breakInterval, true, 14, []));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(60)]
        public void Create_WithHorizonBoundary_ReturnsSettings(int horizonDays)
        {
            var settings = SalonSettings.Create(Address, Phone, WorkInterval, BreakInterval, true, horizonDays, []);
            Assert.Equal(horizonDays, settings.HorizonDays);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(61)]
        public void Create_WhenHorizonOutOfRange_ThrowsBusinessException(int horizonDays)
            => Assert.Throws<BusinessException>(() =>
                SalonSettings.Create(Address, Phone, WorkInterval, BreakInterval, true, horizonDays, []));

        [Fact]
        public void Create_WithSixWeekendDays_ReturnsSettings()
        {
            var weekendDays = new[]
            {
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday
            };
            var settings = SalonSettings.Create(Address, Phone, WorkInterval, BreakInterval, true, 14, weekendDays);
            Assert.Equal(weekendDays, settings.WeekendDays);
        }

        [Fact]
        public void Create_WithSevenWeekendDays_ThrowsBusinessException()
        {
            var weekendDays = Enum.GetValues<DayOfWeek>();
            Assert.Throws<BusinessException>(() =>
                SalonSettings.Create(Address, Phone, WorkInterval, BreakInterval, true, 14, weekendDays));
        }

        [Fact]
        public void Create_WithDuplicateWeekendDays_ThrowsBusinessException()
            => Assert.Throws<BusinessException>(() =>
                SalonSettings.Create(Address, Phone, WorkInterval, BreakInterval, true, 14,
                    [DayOfWeek.Sunday, DayOfWeek.Sunday]));

        [Theory]
        [InlineData(-1)]
        [InlineData(7)]
        public void Create_WhenWeekendDayNotDefined_ThrowsBusinessException(int day)
            => Assert.Throws<BusinessException>(() =>
                SalonSettings.Create(Address, Phone, WorkInterval, BreakInterval, true, 14, [(DayOfWeek)day]));
    }

    public class Update
    {
        [Fact]
        public void Update_WithValid_ChangesSettings()
        {
            var settings = CreateValid();
            var workInterval = TimeInterval.Create(new TimeOnly(9, 0, 0), new TimeOnly(20, 0, 0));
            var breakInterval = TimeInterval.Create(new TimeOnly(13, 0, 0), new TimeOnly(14, 0, 0));

            settings.Update("ул. Новая, 2", "+79341234567", workInterval, breakInterval, false, 30,
                [DayOfWeek.Saturday, DayOfWeek.Sunday]);

            Assert.Equal("ул. Новая, 2", settings.Address);
            Assert.Equal("+79341234567", settings.Phone);
            Assert.Equal(workInterval, settings.WorkInterval);
            Assert.Equal(breakInterval, settings.BreakInterval);
            Assert.False(settings.GenerationEnabled);
            Assert.Equal(30, settings.HorizonDays);
            Assert.Equal(new[] { DayOfWeek.Saturday, DayOfWeek.Sunday }, settings.WeekendDays);
        }

        [Fact]
        public void Update_WhenInvalid_KeepsOldValues()
        {
            var settings = CreateValid();
            Assert.Throws<BusinessException>(() =>
                settings.Update("ул. Новая, 2", Phone, WorkInterval, BreakInterval, false, 61, []));
            Assert.Equal(Address, settings.Address);
            Assert.Equal(14, settings.HorizonDays);
            Assert.True(settings.GenerationEnabled);
        }

        [Fact]
        public void Update_WhenPhoneInvalid_ThrowsBusinessException()
        {
            var settings = CreateValid();
            Assert.Throws<BusinessException>(() =>
                settings.Update(Address, "89991234567", WorkInterval, BreakInterval, true, 14, []));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(7)]
        public void Update_WhenWeekendDayNotDefined_ThrowsBusinessException(int day)
        {
            var settings = CreateValid();
            Assert.Throws<BusinessException>(() =>
                settings.Update(Address, Phone, WorkInterval, BreakInterval, true, 14, [(DayOfWeek)day]));
        }
    }
}
