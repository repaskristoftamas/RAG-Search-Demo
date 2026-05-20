using BookStore.Catalog.Domain.Books;
using BookStore.Catalog.IntegrationTests.Infrastructure;
using Microsoft.Extensions.Time.Testing;
using Shouldly;

namespace BookStore.Catalog.IntegrationTests.Data;

[Collection(nameof(PostgreSqlCollection))]
public sealed class AuditTimestampTests(PostgreSqlFixture fixture) : CatalogIntegrationTestBase(fixture)
{
    private static readonly DateTimeOffset InitialTime = new(2026, 5, 19, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset UpdateTime = new(2026, 5, 19, 13, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task SaveChanges_NewEntity_SetsCreatedAtFromTimeProvider()
    {
        var fakeTime = new FakeTimeProvider(InitialTime);
        var book = Book.Create("Title", "Author", "Description").Value;

        await using (var context = CreateContext(timeProvider: fakeTime))
        {
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var persisted = await context.Books.FindAsync(book.Id);

            persisted!.CreatedAt.ShouldBe(InitialTime);
            persisted.UpdatedAt.ShouldBeNull();
        }
    }

    [Fact]
    public async Task SaveChanges_ModifiedEntity_SetsUpdatedAtFromTimeProvider()
    {
        var fakeTime = new FakeTimeProvider(InitialTime);
        var book = Book.Create("Title", "Author", "Description").Value;

        await using (var context = CreateContext(timeProvider: fakeTime))
        {
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        fakeTime.SetUtcNow(UpdateTime);

        await using (var context = CreateContext(timeProvider: fakeTime))
        {
            var tracked = await context.Books.FindAsync(book.Id);
            tracked!.Update("Updated", "Author", "Description");
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var persisted = await context.Books.FindAsync(book.Id);

            persisted!.CreatedAt.ShouldBe(InitialTime);
            persisted.UpdatedAt.ShouldBe(UpdateTime);
        }
    }

    [Fact]
    public async Task SaveChanges_ModifiedEntity_DoesNotOverwriteCreatedAt()
    {
        var fakeTime = new FakeTimeProvider(InitialTime);
        var book = Book.Create("Title", "Author", "Description").Value;

        await using (var context = CreateContext(timeProvider: fakeTime))
        {
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        fakeTime.SetUtcNow(UpdateTime);

        await using (var context = CreateContext(timeProvider: fakeTime))
        {
            var tracked = await context.Books.FindAsync(book.Id);
            tracked!.Update("Updated", "Author", "Description");
            await context.SaveChangesAsync();
        }

        await using (var context = CreateContext())
        {
            var persisted = await context.Books.FindAsync(book.Id);
            persisted!.CreatedAt.ShouldBe(InitialTime);
        }
    }
}
