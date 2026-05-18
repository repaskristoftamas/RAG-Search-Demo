using BookStore.KeywordSearch.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStore.KeywordSearch.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="SearchableBook"/> read model.
/// </summary>
internal sealed class SearchableBookConfiguration : IEntityTypeConfiguration<SearchableBook>
{
    /// <summary>
    /// Configures the SearchableBooks table schema.
    /// </summary>
    public void Configure(EntityTypeBuilder<SearchableBook> builder)
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

        builder.Property(b => b.IndexedAt)
            .IsRequired();
    }
}
