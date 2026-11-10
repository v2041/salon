using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class ScheduleConfiguration : IEntityTypeConfiguration<Schedule>
{
    public void Configure(EntityTypeBuilder<Schedule> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(s => s.Date).IsUnique();
        builder.Property(s => s.Source).HasDefaultValue(ScheduleSource.Manual);
        builder.ComplexProperty(a => a.WorkInterval,
            interval =>
            {
                interval.Property(p => p.Start).HasColumnName("WorkStartTime");
                interval.Property(p => p.End).HasColumnName("WorkEndTime");
            }
        );
        builder.ComplexProperty(a => a.BreakInterval,
            interval =>
            {
                interval.Property(p => p.Start).HasColumnName("BreakStartTime");
                interval.Property(p => p.End).HasColumnName("BreakEndTime");
            }
        );
    }
}