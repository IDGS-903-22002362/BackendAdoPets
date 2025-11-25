using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Chat;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Chat;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ConversationId)
            .IsRequired();

        builder.Property(m => m.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.Content)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2");

        // Índices para mejorar las consultas
        builder.HasIndex(m => m.ConversationId)
            .HasDatabaseName("IX_ChatMessage_ConversationId");

        builder.HasIndex(m => new { m.ConversationId, m.CreatedAt })
            .HasDatabaseName("IX_ChatMessage_ConversationId_CreatedAt");

        // Relación con ChatConversation (ya configurada desde el lado padre)
    }
}
