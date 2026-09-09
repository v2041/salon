using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Phone).IsUnique();

        builder.HasData(
            User.Create(
                "Admin",
                "Test",
                "+79991234567",
                DateTime.UtcNow
            )
        );
        builder.HasData(
            User.Create(
                "Client",
                "Test",
                "+79341234567",
                DateTime.UtcNow
            )
        );
    }
}