using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AdoPetsBKD.Domain.Entities.Chat;

namespace AdoPetsBKD.Infrastructure.Data.Configurations.Chat;

public class ChatConversationConfiguration : IEntityTypeConfiguration<ChatConversation>
{
    public void Configure(EntityTypeBuilder<ChatConversation> builder)
    {
        builder.ToTable("ChatConversations");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(c => c.CreatedAt)
            .IsRequired()
            .HasColumnType("datetime2");

        // Índices para mejorar las consultas
        builder.HasIndex(c => c.UserId)
            .HasDatabaseName("IX_ChatConversation_UserId");

        builder.HasIndex(c => c.CreatedAt)
            .HasDatabaseName("IX_ChatConversation_CreatedAt");

        // Relación uno a muchos con ChatMessages
        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
