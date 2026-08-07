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
                "Admin",
                "Test",
                "admin",
                "+79991234567",
                DateTime.Now
            )
        );
        builder.HasData(
            User.Create(
                "Client",
                "Test",
                "default",
                "+79341234567",
                DateTime.Now
            )
        );
    }
}