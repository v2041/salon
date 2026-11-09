using Domain.Entities;
using Domain.Enums;
using Domain.Services;
using Domain.ValueObjects;

namespace DomainTests.Services;

public class SchedulePlannerTests
{
    // 2030-05-06 — понедельник. Воскресенье в диапазоне — 2030-05-12.
    private static readonly DateOnly Today = new(2030, 5, 6);

    private static readonly TimeInterval WorkInterval =
        TimeInterval.Create(new TimeOnly(10, 0, 0), new TimeOnly(18, 0, 0));

    private static readonly TimeInterval BreakInterval =
        TimeInterval.Create(new TimeOnly(15, 0, 0), new TimeOnly(15, 20, 0));

    private static readonly TimeInterval OtherWorkInterval =
        TimeInterval.Create(new TimeOnly(9, 0, 0), new TimeOnly(13, 0, 0));

    private static SalonSettings CreateSettings(
        bool generationEnabled = true,
        int horizonDays = 7,
        params DayOfWeek[] weekendDays)
    {
        var days = weekendDays.Length == 0 ? [DayOfWeek.Sunday] : weekendDays;
        return SalonSettings.Create(
            "ул. Тверская, 1",
            "+79991234567",
            WorkInterval,
            BreakInterval,
            generationEnabled,
            horizonDays,
            days);
    }

    private static ScheduleDayState CreateDay(
        DateOnly date,
        bool isWorking = true,
        ScheduleSource source = ScheduleSource.Auto,
        bool hasActiveAppointments = false,
        TimeInterval? workInterval = null,
        bool hasBreak = true) =>
        new(date, source, isWorking, workInterval ?? WorkInterval, hasBreak ? BreakInterval : null,
            hasActiveAppointments);

    private static SchedulePlanItem Find(IReadOnlyList<SchedulePlanItem> plan, DateOnly date) =>
        Assert.Single(plan, item => item.Date == date);

    public class BuildGenerationPlan
    {
        [Fact]
        public void WhenGenerationDisabled_ReturnsEmptyPlan()
        {
            var settings = CreateSettings(generationEnabled: false, horizonDays: 3);

            var plan = SchedulePlanner.BuildGenerationPlan(settings, [], Today);

            Assert.Empty(plan);
        }

        [Fact]
        public void CreatesMissingDaysFromTomorrowToHorizon()
        {
            var settings = CreateSettings(horizonDays: 3);

            var plan = SchedulePlanner.BuildGenerationPlan(settings, [], Today);

            Assert.Equal(
                new[] { Today.AddDays(1), Today.AddDays(2), Today.AddDays(3) },
                plan.Select(item => item.Date));
            Assert.All(plan, item => Assert.Equal(SchedulePlanAction.Create, item.Action));
            var target = plan[0].Target!;
            Assert.True(target.IsWorking);
            Assert.Equal(WorkInterval, target.WorkInterval);
            Assert.Equal(BreakInterval, target.BreakInterval);
        }

        [Fact]
        public void WhenWeekendDay_CreatesNonWorkingDayWithSettingsIntervals()
        {
            var settings = CreateSettings(horizonDays: 7, weekendDays: [DayOfWeek.Sunday]);

            var plan = SchedulePlanner.BuildGenerationPlan(settings, [], Today);

            var sunday = Find(plan, Today.AddDays(6));
            Assert.Equal(SchedulePlanAction.Create, sunday.Action);
            Assert.False(sunday.Target!.IsWorking);
            Assert.Equal(WorkInterval, sunday.Target.WorkInterval);
            Assert.Equal(BreakInterval, sunday.Target.BreakInterval);
        }

        [Fact]
        public void DoesNotTouchTodayOrDaysAfterHorizon()
        {
            var settings = CreateSettings(horizonDays: 3);
            var existing = new[]
            {
                CreateDay(Today),
                CreateDay(Today.AddDays(4))
            };

            var plan = SchedulePlanner.BuildGenerationPlan(settings, existing, Today);

            Assert.Equal(3, plan.Count);
            Assert.DoesNotContain(plan, item => item.Date == Today);
            Assert.DoesNotContain(plan, item => item.Date == Today.AddDays(4));
        }

        [Fact]
        public void WhenDayIsManual_SkipsIt()
        {
            var settings = CreateSettings(horizonDays: 1);
            var date = Today.AddDays(1);
            var existing = CreateDay(date, source: ScheduleSource.Manual, workInterval: OtherWorkInterval,
                hasBreak: false);

            var plan = SchedulePlanner.BuildGenerationPlan(settings, [existing], Today);

            var item = Find(plan, date);
            Assert.Equal(SchedulePlanAction.Skip, item.Action);
            Assert.Equal(SchedulePlanSkipReason.Manual, item.SkipReason);
            Assert.Null(item.Target);
        }

        [Fact]
        public void WhenDayHasActiveAppointments_SkipsIt()
        {
            var settings = CreateSettings(horizonDays: 1);
            var date = Today.AddDays(1);
            var existing = CreateDay(date, hasActiveAppointments: true, workInterval: OtherWorkInterval,
                hasBreak: false);

            var plan = SchedulePlanner.BuildGenerationPlan(settings, [existing], Today);

            var item = Find(plan, date);
            Assert.Equal(SchedulePlanAction.Skip, item.Action);
            Assert.Equal(SchedulePlanSkipReason.HasActiveAppointments, item.SkipReason);
        }

        [Fact]
        public void WhenAutoDayDiffers_ChangesIt()
        {
            var settings = CreateSettings(horizonDays: 1);
            var date = Today.AddDays(1);
            var existing = CreateDay(date, workInterval: OtherWorkInterval, hasBreak: false);

            var plan = SchedulePlanner.BuildGenerationPlan(settings, [existing], Today);

            var item = Find(plan, date);
            Assert.Equal(SchedulePlanAction.Change, item.Action);
            Assert.True(item.Target!.IsWorking);
            Assert.Equal(WorkInterval, item.Target.WorkInterval);
            Assert.Equal(BreakInterval, item.Target.BreakInterval);
        }

        [Fact]
        public void WhenAutoDayIsNonWorking_ChangesIt()
        {
            var settings = CreateSettings(horizonDays: 1);
            var date = Today.AddDays(1);
            var existing = CreateDay(date, isWorking: false);

            var plan = SchedulePlanner.BuildGenerationPlan(settings, [existing], Today);

            var item = Find(plan, date);
            Assert.Equal(SchedulePlanAction.Change, item.Action);
            Assert.True(item.Target!.IsWorking);
        }

        [Fact]
        public void WhenAutoDayMatches_SkipsIt()
        {
            var settings = CreateSettings(horizonDays: 1);
            var date = Today.AddDays(1);
            var existing = CreateDay(date);

            var plan = SchedulePlanner.BuildGenerationPlan(settings, [existing], Today);

            var item = Find(plan, date);
            Assert.Equal(SchedulePlanAction.Skip, item.Action);
            Assert.Equal(SchedulePlanSkipReason.AlreadyMatches, item.SkipReason);
        }
    }

    public class BuildApplyPlan
    {
        [Fact]
        public void WhenGenerationDisabled_StillBuildsPlan()
        {
            var settings = CreateSettings(generationEnabled: false, horizonDays: 1);

            var plan = SchedulePlanner.BuildApplyPlan(settings, [], Today);

            var item = Find(plan, Today.AddDays(1));
            Assert.Equal(SchedulePlanAction.Create, item.Action);
        }

        [Fact]
        public void CreatesMissingDaysUpToHorizon()
        {
            var settings = CreateSettings(horizonDays: 3);

            var plan = SchedulePlanner.BuildApplyPlan(settings, [], Today);

            Assert.Equal(
                new[] { Today.AddDays(1), Today.AddDays(2), Today.AddDays(3) },
                plan.Select(item => item.Date));
            Assert.All(plan, item => Assert.Equal(SchedulePlanAction.Create, item.Action));
        }

        [Fact]
        public void WhenManualDayWithoutAppointments_ChangesIt()
        {
            var settings = CreateSettings(horizonDays: 1);
            var date = Today.AddDays(1);
            var existing = CreateDay(date, source: ScheduleSource.Manual);

            var plan = SchedulePlanner.BuildApplyPlan(settings, [existing], Today);

            var item = Find(plan, date);
            Assert.Equal(SchedulePlanAction.Change, item.Action);
            Assert.Equal(WorkInterval, item.Target!.WorkInterval);
            Assert.Equal(BreakInterval, item.Target.BreakInterval);
        }

        [Fact]
        public void WhenAutoDayBeyondHorizonDiffers_ChangesIt()
        {
            var settings = CreateSettings(horizonDays: 1);
            var date = Today.AddDays(10);
            var existing = CreateDay(date, workInterval: OtherWorkInterval, hasBreak: false);

            var plan = SchedulePlanner.BuildApplyPlan(settings, [existing], Today);

            var item = Find(plan, date);
            Assert.Equal(SchedulePlanAction.Change, item.Action);
            Assert.Equal(WorkInterval, item.Target!.WorkInterval);
        }

        [Fact]
        public void WhenDayHasActiveAppointments_SkipsIt()
        {
            var settings = CreateSettings(horizonDays: 1);
            var date = Today.AddDays(1);
            var existing = CreateDay(date, source: ScheduleSource.Manual, hasActiveAppointments: true);

            var plan = SchedulePlanner.BuildApplyPlan(settings, [existing], Today);

            var item = Find(plan, date);
            Assert.Equal(SchedulePlanAction.Skip, item.Action);
            Assert.Equal(SchedulePlanSkipReason.HasActiveAppointments, item.SkipReason);
        }

        [Fact]
        public void DoesNotTouchToday()
        {
            var settings = CreateSettings(horizonDays: 1);
            var existing = CreateDay(Today, workInterval: OtherWorkInterval, hasBreak: false);

            var plan = SchedulePlanner.BuildApplyPlan(settings, [existing], Today);

            Assert.DoesNotContain(plan, item => item.Date == Today);
        }

        [Fact]
        public void WhenAutoDayMatches_SkipsIt()
        {
            var settings = CreateSettings(horizonDays: 1);
            var date = Today.AddDays(1);
            var existing = CreateDay(date);

            var plan = SchedulePlanner.BuildApplyPlan(settings, [existing], Today);

            var item = Find(plan, date);
            Assert.Equal(SchedulePlanAction.Skip, item.Action);
            Assert.Equal(SchedulePlanSkipReason.AlreadyMatches, item.SkipReason);
        }
    }

    public class Apply
    {
        [Fact]
        public void Create_ReturnsScheduleWithAutoSource()
        {
            var date = Today.AddDays(1);
            var plan = new[]
            {
                SchedulePlanItem.Create(date, new SchedulePlanTarget(true, WorkInterval, BreakInterval))
            };

            var created = SchedulePlanner.Apply(plan, new Dictionary<DateOnly, Schedule>());

            var schedule = Assert.Single(created);
            Assert.Equal(date, schedule.Date);
            Assert.True(schedule.IsWorking);
            Assert.Equal(WorkInterval, schedule.WorkInterval);
            Assert.Equal(BreakInterval, schedule.BreakInterval);
            Assert.Equal(ScheduleSource.Auto, schedule.Source);
        }

        [Fact]
        public void Change_AlignsExistingDayAndMarksAuto()
        {
            var date = Today.AddDays(1);
            var schedule = Schedule.Create(date, true, OtherWorkInterval, null, checkPast: false);
            var plan = new[]
            {
                SchedulePlanItem.Change(date, new SchedulePlanTarget(true, WorkInterval, BreakInterval))
            };
            var existing = new Dictionary<DateOnly, Schedule> { [date] = schedule };

            var created = SchedulePlanner.Apply(plan, existing);

            Assert.Empty(created);
            Assert.Equal(WorkInterval, schedule.WorkInterval);
            Assert.Equal(BreakInterval, schedule.BreakInterval);
            Assert.Equal(ScheduleSource.Auto, schedule.Source);
        }

        [Fact]
        public void Skip_DoesNotTouchExistingDay()
        {
            var date = Today.AddDays(1);
            var schedule = Schedule.Create(date, true, OtherWorkInterval, null, checkPast: false);
            var plan = new[] { SchedulePlanItem.Skip(date, SchedulePlanSkipReason.Manual) };
            var existing = new Dictionary<DateOnly, Schedule> { [date] = schedule };

            var created = SchedulePlanner.Apply(plan, existing);

            Assert.Empty(created);
            Assert.Equal(OtherWorkInterval, schedule.WorkInterval);
            Assert.Equal(ScheduleSource.Manual, schedule.Source);
        }
    }
}
