using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class SalonSettingsConfiguration : IEntityTypeConfiguration<SalonSettings>
{
    // Id сида зафиксирован: строка настроек одна, Id не должен генериться заново при каждой сборке модели.
    private static readonly Guid SeedId = Guid.Parse("7f2b9d64-3c1a-4e58-9b0d-6a5f4e3d2c1b");

    public void Configure(EntityTypeBuilder<SalonSettings> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Address)
            .IsRequired()
            .HasMaxLength(SalonSettings.MAX_ADDRESS_LENGTH);
        builder.Property(x => x.Phone)
            .IsRequired()
            .HasMaxLength(12);
        builder.Property(x => x.WeekendDays).HasField("_weekendDays");
        builder.OwnsOne(x => x.WorkInterval,
            interval =>
            {
                interval.Property(p => p.Start).HasColumnName("WorkStartTime");
                interval.Property(p => p.End).HasColumnName("WorkEndTime");
                interval.HasData(new
                {
                    SalonSettingsId = SeedId,
                    Start = new TimeOnly(10, 0, 0),
                    End = new TimeOnly(18, 0, 0)
                });
            }
        );
        builder.OwnsOne(x => x.BreakInterval,
            interval =>
            {
                interval.Property(p => p.Start).HasColumnName("BreakStartTime");
                interval.Property(p => p.End).HasColumnName("BreakEndTime");
                interval.HasData(new
                {
                    SalonSettingsId = SeedId,
                    Start = new TimeOnly(15, 0, 0),
                    End = new TimeOnly(15, 20, 0)
                });
            }
        );
        builder.HasData(new
        {
            Id = SeedId,
            Address = "ул. Примерная, д. 1",
            Phone = "+79991234567",
            GenerationEnabled = true,
            HorizonDays = 14,
            WeekendDays = new List<DayOfWeek> { DayOfWeek.Sunday }
        });
    }
}
