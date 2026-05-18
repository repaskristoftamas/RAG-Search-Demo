using BookStore.Catalog.Application.Abstractions;
using BookStore.Catalog.Domain.Books;
using BookStore.SharedKernel.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Catalog.Application.Books.Commands.SeedBooks;

/// <summary>
/// Handles seeding the catalog with sample books, skipping any that already exist by title.
/// </summary>
internal sealed class SeedBooksCommandHandler(
    ICatalogDbContext context) : ICommandHandler<SeedBooksCommand, Result<int>>
{
    /// <summary>
    /// Seeds the catalog and returns the number of books inserted.
    /// </summary>
    public async ValueTask<Result<int>> Handle(SeedBooksCommand command, CancellationToken cancellationToken)
    {
        var existingTitles = await context.Books
            .Select(b => b.Title)
            .ToListAsync(cancellationToken);

        var existingTitleSet = existingTitles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var inserted = 0;

        foreach (var item in command.Books)
        {
            if (existingTitleSet.Contains(item.Title))
                continue;

            var createResult = Book.Create(item.Title, item.Author, item.Description);
            if (createResult.IsFailure)
                continue;

            context.Books.Add(createResult.Value);
            inserted++;
        }

        if (inserted > 0)
            await context.SaveChangesAsync(cancellationToken);

        return Result.Success(inserted);
    }
}
