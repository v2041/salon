using Domain.Entities;
using Infrastructure.Configurations;
using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class SalonDbContext(DbContextOptions<SalonDbContext> options)
    : DbContext(options)
{
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<Offering> Offerings { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<User> Users { get; set; }
    
    public DbSet<VerificationCode> VerificationCodes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AppointmentConfiguration());
        modelBuilder.ApplyConfiguration(new AppointmentOfferingConfiguration());
        modelBuilder.ApplyConfiguration(new OfferingConfiguration());
        modelBuilder.ApplyConfiguration(new ScheduleConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new VerificationCodeConfiguration());
    }

    public override Task<int> SaveChangesAsync(CancellationToken token = default)
    {
        var entries = ChangeTracker
            .Entries<Appointment>()
            .Where(e => e.State == EntityState.Modified);

        // foreach (var entry in entries)
        // {
        //     entry.Property("Version").CurrentValue = Guid.NewGuid();
        // }
        return base.SaveChangesAsync(token);
    }
}