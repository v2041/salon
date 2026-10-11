using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class AppointmentOfferingConfiguration : IEntityTypeConfiguration<AppointmentOffering>
{
    public void Configure(EntityTypeBuilder<AppointmentOffering> builder)
    {
        builder.ToTable("AppointmentOffering");
        builder.HasKey(a => new {a.AppointmentId, a.OfferingId});
        builder.Property(a => a.AppointmentId).ValueGeneratedNever();
        builder.Property(a => a.OfferingId).ValueGeneratedNever();
        builder.OwnsOne(a => a.Price, moneyBuilder =>
        {
            moneyBuilder.Property(m => m.Value)
                .HasColumnName("Price")
                .HasPrecision(18, 2);
        });
    }
}