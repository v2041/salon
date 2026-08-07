using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(a => a.UserId);
        builder.HasOne(a => a.Schedule)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.ScheduleId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(a => a.Offerings)
            .WithOne();
        builder.OwnsOne(a => a.Interval,
            interval =>
            {
                interval.Property(p => p.Start).HasColumnName("StartTime");
                interval.Property(p => p.End).HasColumnName("EndTime");
            }
        );
    }
}