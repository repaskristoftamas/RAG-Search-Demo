using BookStore.SemanticSearch.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStore.SemanticSearch.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="BookEmbedding"/> read model.
/// </summary>
internal sealed class BookEmbeddingConfiguration : IEntityTypeConfiguration<BookEmbedding>
{
    /// <summary>
    /// Configures the BookEmbeddings table schema with pgvector column.
    /// </summary>
    public void Configure(EntityTypeBuilder<BookEmbedding> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(b => b.Description)
            .IsRequired();

        builder.Property(b => b.Embedding)
            .HasColumnType("vector(768)")
            .IsRequired();

        builder.Property(b => b.IndexedAt)
            .IsRequired();

        builder.HasIndex(b => b.Embedding)
            .HasMethod("ivfflat")
            .HasOperators("vector_cosine_ops")
            .HasStorageParameter("lists", 10);
    }
}
