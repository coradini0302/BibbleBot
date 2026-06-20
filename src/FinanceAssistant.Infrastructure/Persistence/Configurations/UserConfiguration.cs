using FinanceAssistant.Domain.Entities;
using FinanceAssistant.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceAssistant.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.TelegramId).IsRequired();
        builder.HasIndex(u => u.TelegramId).IsUnique();

        builder.Property(u => u.Name).IsRequired().HasMaxLength(200);

        builder.Property(u => u.CreatedAt)
            .IsRequired()
            .HasColumnType("timestamp with time zone");

        builder.Property(u => u.ReportDay);

        builder.Property(u => u.ReportFormat)
            .IsRequired()
            .HasDefaultValue(ReportFormat.Excel);
    }
}
