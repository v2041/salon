using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class OfferingConfiguration : IEntityTypeConfiguration<Offering>
{
    public void Configure(EntityTypeBuilder<Offering> builder)
    {
        builder.HasKey(x => x.Id);
        builder.OwnsOne(o => o.Price, priceBuilder =>
        {
            priceBuilder.Property(m => m.Value).HasColumnName("Price");
        });
    }
}