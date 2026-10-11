using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    // Id сидов зафиксированы: это Id уже созданных строк в БД.
    // Менять их нельзя — на Users ссылаются записи (Appointments.UserId).
    // Фиксированные Id и даты также останавливают мусорный DeleteData/InsertData
    // в каждой новой миграции (раньше Id и даты генерились заново при каждой сборке модели).
    private static readonly Guid AdminSeedId = Guid.Parse("d4c60e8f-86b8-4634-8008-9c78df7e7347");
    private static readonly Guid ClientSeedId = Guid.Parse("ff9152cd-b1b1-4218-9cdf-d80155e145d8");
    private static readonly DateTime SeedDate = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Phone).IsUnique();

        var admin = User.Create(
            "Admin",
            "Test",
            "+79991234567",
            SeedDate,
            AdminSeedId
        );
        admin.PromoteToAdmin();
        builder.HasData(admin);

        builder.HasData(
            User.Create(
                "Client",
                "Test",
                "+79341234567",
                SeedDate,
                ClientSeedId
            )
        );
    }
}