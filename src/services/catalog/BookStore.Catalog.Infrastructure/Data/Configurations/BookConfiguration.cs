using BookStore.Catalog.Domain.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BookStore.Catalog.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the <see cref="Book"/> entity.
/// </summary>
internal sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    /// <summary>
    /// Configures the Book table schema.
    /// </summary>
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        var converter = new ValueConverter<BookId, Guid>(
            id => id.Value,
            guid => new BookId(guid));

        var comparer = new ValueComparer<BookId>(
            (a, b) => a.Value == b.Value,
            id => id.Value.GetHashCode(),
            id => new BookId(id.Value));

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasConversion(converter, comparer);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(b => b.Description)
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        builder.Property(b => b.UpdatedAt);
    }
}
