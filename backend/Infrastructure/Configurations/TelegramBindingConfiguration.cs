using Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Configurations;

public class TelegramBindingConfiguration : IEntityTypeConfiguration<TelegramBinding>
{
    public void Configure(EntityTypeBuilder<TelegramBinding> builder)
    {
        builder.HasKey(x => x.Phone);
    }
}
