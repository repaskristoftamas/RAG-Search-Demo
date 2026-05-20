using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BookStore.EndToEndTests.Infrastructure;
using Shouldly;

namespace BookStore.EndToEndTests.CatalogApi;

/// <summary>
/// Tests the Catalog API book CRUD endpoints through the full HTTP pipeline.
/// </summary>
[Collection(nameof(CatalogApiCollection))]
public sealed class BookCrudTests(CatalogApiFactory factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateBook_ValidRequest_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/books", new
        {
            Title = "Clean Code",
            Author = "Robert C. Martin",
            Description = "A handbook of agile software craftsmanship."
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.Headers.Location.ShouldNotBeNull();

        var bookId = await response.Content.ReadFromJsonAsync<Guid>();
        bookId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task CreateBook_EmptyTitle_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/books", new
        {
            Title = "",
            Author = "Author",
            Description = "Description"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.ShouldContain("VALIDATION_FAILED");
    }

    [Fact]
    public async Task GetBookById_ExistingBook_ReturnsBook()
    {
        var bookId = await CreateBookAsync("Domain-Driven Design", "Eric Evans", "Tackling complexity.");

        var response = await _client.GetAsync($"/api/v1/books/{bookId}");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("title").GetString().ShouldBe("Domain-Driven Design");
        doc.RootElement.GetProperty("author").GetString().ShouldBe("Eric Evans");
    }

    [Fact]
    public async Task GetBookById_NonExistentId_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/v1/books/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllBooks_ReturnsPagedResult()
    {
        await CreateBookAsync("Paged Book A", "Author A", "Description A");
        await CreateBookAsync("Paged Book B", "Author B", "Description B");

        var response = await _client.GetAsync("/api/v1/books?page=1&pageSize=50");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("items").GetArrayLength().ShouldBeGreaterThanOrEqualTo(2);
        doc.RootElement.GetProperty("totalCount").GetInt32().ShouldBeGreaterThanOrEqualTo(2);
        doc.RootElement.GetProperty("page").GetInt32().ShouldBe(1);
    }

    [Fact]
    public async Task UpdateBook_ValidRequest_ReturnsNoContent()
    {
        var bookId = await CreateBookAsync("Original Title", "Author", "Description");

        var response = await _client.PutAsJsonAsync($"/api/v1/books/{bookId}", new
        {
            Title = "Updated Title",
            Author = "Updated Author",
            Description = "Updated Description"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/v1/books/{bookId}");
        var doc = JsonDocument.Parse(await getResponse.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("title").GetString().ShouldBe("Updated Title");
    }

    [Fact]
    public async Task UpdateBook_NonExistentId_ReturnsNotFound()
    {
        var response = await _client.PutAsJsonAsync($"/api/v1/books/{Guid.NewGuid()}", new
        {
            Title = "Title",
            Author = "Author",
            Description = "Description"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBook_ExistingBook_ReturnsNoContent()
    {
        var bookId = await CreateBookAsync("To Be Deleted", "Author", "Description");

        var response = await _client.DeleteAsync($"/api/v1/books/{bookId}");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/v1/books/{bookId}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBook_NonExistentId_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync($"/api/v1/books/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SeedBooks_ValidRequest_ReturnsInsertedCount()
    {
        var uniqueSuffix = Guid.NewGuid().ToString()[..8];
        var response = await _client.PostAsJsonAsync("/api/v1/books/seed", new
        {
            Books = new[]
            {
                new { Title = $"Seed A {uniqueSuffix}", Author = "Author A", Description = "Desc A" },
                new { Title = $"Seed B {uniqueSuffix}", Author = "Author B", Description = "Desc B" }
            }
        });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        doc.RootElement.GetProperty("insertedCount").GetInt32().ShouldBe(2);
    }

    /// <summary>
    /// Creates a book via the API and returns its identifier.
    /// </summary>
    private async Task<Guid> CreateBookAsync(string title, string author, string description)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/books", new { Title = title, Author = author, Description = description });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
    }
}
