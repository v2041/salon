using Domain.Entities;
using Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class SalonDbContext(DbContextOptions<SalonDbContext> options)
    : DbContext(options)
{
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Offering> Offerings { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Schedule>()
            .HasIndex(s => s.Date)
            .IsUnique();
        modelBuilder.Entity<Appointment>()
            .OwnsOne(a => a.Interval,
                interval =>
                {
                    interval.Property(p => p.Start).HasColumnName("StartTime");
                    interval.Property(p => p.End).HasColumnName("EndTime");
                }
            );
        modelBuilder.Entity<Schedule>()
            .OwnsOne(a => a.WorkInterval,
                interval =>
                {
                    interval.Property(p => p.Start).HasColumnName("WorkStartTime");
                    interval.Property(p => p.End).HasColumnName("WorkEndTime");
                }
            );
        modelBuilder.Entity<Schedule>()
            .OwnsOne(a => a.BreakInterval,
                interval =>
                {
                    interval.Property(p => p.Start).HasColumnName("BreakStartTime");
                    interval.Property(p => p.End).HasColumnName("BreakEndTime");
                }
            );
        modelBuilder.ApplyConfiguration(new AppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new OfferingConfiguration());
        modelBuilder.ApplyConfiguration(new ScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        //base.OnModelCreating(modelBuilder);
    }
}