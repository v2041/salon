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
        builder.HasData(
            User.Create(
                1234,
                "Admin",
                "null",
                "admin",
                "+79991234567",
                false
            )
        );
        builder.HasData(
            User.Create(
                12345,
                "Client",
                "Test",
                "default",
                "+79341234567",
                false
            )
        );
    }
}