using BookStore.Catalog.Application.Books.DTOs;
using BookStore.Catalog.Domain.Books;
using Riok.Mapperly.Abstractions;

namespace BookStore.Catalog.Application.Books.Mappers;

/// <summary>
/// Converts <see cref="Book"/> domain entities to <see cref="BookDto"/> objects.
/// </summary>
[Mapper]
internal static partial class BookMapper
{
    /// <summary>
    /// Maps a <see cref="Book"/> entity to its DTO representation.
    /// </summary>
    [MapperIgnoreSource(nameof(Book.DomainEvents))]
    public static partial BookDto ToDto(this Book book);

    /// <summary>
    /// Extracts the underlying <see cref="Guid"/> from a <see cref="BookId"/>.
    /// </summary>
    private static Guid MapBookId(BookId id) => id.Value;
}
