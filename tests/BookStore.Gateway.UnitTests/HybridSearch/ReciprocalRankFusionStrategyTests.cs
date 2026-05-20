using BookStore.Gateway.Comparison;
using BookStore.Gateway.HybridSearch;
using Shouldly;

namespace BookStore.Gateway.UnitTests.HybridSearch;

public sealed class ReciprocalRankFusionStrategyTests
{
    private static readonly Guid BookA = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly Guid BookB = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    private static readonly Guid BookC = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");
    private static readonly Guid BookD = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

    private readonly ReciprocalRankFusionStrategy _sut = new();

    [Fact]
    public void Fuse_OverlappingResults_RanksSharedBooksHigher()
    {
        var keyword = CreateResults((BookA, "Book A"), (BookB, "Book B"));
        var semantic = CreateResults((BookB, "Book B"), (BookC, "Book C"));

        var result = _sut.Fuse(keyword, semantic, 10, 0.5, 0.5);

        result[0].BookId.ShouldBe(BookB);
    }

    [Fact]
    public void Fuse_DisjointResults_MergesAllBooks()
    {
        var keyword = CreateResults((BookA, "Book A"), (BookB, "Book B"));
        var semantic = CreateResults((BookC, "Book C"), (BookD, "Book D"));

        var result = _sut.Fuse(keyword, semantic, 10, 0.5, 0.5);

        result.Count.ShouldBe(4);
        result.Select(r => r.BookId).ShouldBe(
            [BookA, BookC, BookB, BookD], ignoreOrder: false);
    }

    [Fact]
    public void Fuse_EmptyKeywordList_ReturnsSemanticsOnly()
    {
        var keyword = Array.Empty<SearchResultItem>();
        var semantic = CreateResults((BookA, "Book A"), (BookB, "Book B"));

        var result = _sut.Fuse(keyword, semantic, 10, 0.5, 0.5);

        result.Count.ShouldBe(2);
        result.ShouldAllBe(r => r.KeywordRank == null);
        result.ShouldAllBe(r => r.SemanticRank != null);
    }

    [Fact]
    public void Fuse_EmptySemanticList_ReturnsKeywordsOnly()
    {
        var keyword = CreateResults((BookA, "Book A"), (BookB, "Book B"));
        var semantic = Array.Empty<SearchResultItem>();

        var result = _sut.Fuse(keyword, semantic, 10, 0.5, 0.5);

        result.Count.ShouldBe(2);
        result.ShouldAllBe(r => r.SemanticRank == null);
        result.ShouldAllBe(r => r.KeywordRank != null);
    }

    [Fact]
    public void Fuse_BothListsEmpty_ReturnsEmptyList()
    {
        var result = _sut.Fuse([], [], 10, 0.5, 0.5);

        result.ShouldBeEmpty();
    }

    [Fact]
    public void Fuse_MaxResultsLimitsOutput()
    {
        var keyword = CreateResults(
            (BookA, "Book A"), (BookB, "Book B"), (BookC, "Book C"));
        var semantic = CreateResults(
            (BookA, "Book A"), (BookB, "Book B"), (BookD, "Book D"));

        var result = _sut.Fuse(keyword, semantic, 2, 0.5, 0.5);

        result.Count.ShouldBe(2);
    }

    [Fact]
    public void Fuse_CustomWeights_AffectsRanking()
    {
        var keyword = CreateResults((BookA, "Book A"));
        var semantic = CreateResults((BookB, "Book B"));

        var keywordHeavy = _sut.Fuse(keyword, semantic, 10, 0.9, 0.1);
        var semanticHeavy = _sut.Fuse(keyword, semantic, 10, 0.1, 0.9);

        keywordHeavy[0].BookId.ShouldBe(BookA);
        semanticHeavy[0].BookId.ShouldBe(BookB);
    }

    [Fact]
    public void Fuse_WeightsNormalizeToOne()
    {
        var keyword = CreateResults((BookA, "Book A"));
        var semantic = CreateResults((BookB, "Book B"));

        var resultSmall = _sut.Fuse(keyword, semantic, 10, 0.4, 0.6);
        var resultLarge = _sut.Fuse(keyword, semantic, 10, 2.0, 3.0);

        resultSmall.Count.ShouldBe(resultLarge.Count);
        for (var i = 0; i < resultSmall.Count; i++)
        {
            resultSmall[i].BookId.ShouldBe(resultLarge[i].BookId);
            resultSmall[i].FusedScore.ShouldBe(resultLarge[i].FusedScore, tolerance: 1e-10);
        }
    }

    [Fact]
    public void Fuse_ZeroWeights_ThrowsArgumentException()
    {
        Should.Throw<ArgumentException>(() => _sut.Fuse([], [], 10, 0, 0));
    }

    [Fact]
    public void Fuse_NegativeWeight_ThrowsArgumentOutOfRangeException()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => _sut.Fuse([], [], 10, -1, 0.5));
        Should.Throw<ArgumentOutOfRangeException>(() => _sut.Fuse([], [], 10, 0.5, -1));
    }

    [Fact]
    public void Fuse_PreservesBookMetadata()
    {
        var keyword = new List<SearchResultItem>
        {
            new(BookA, "The Title", "The Author", "The Description", 0.95)
        };

        var result = _sut.Fuse(keyword, [], 10, 1.0, 0.0);

        var item = result.ShouldHaveSingleItem();
        item.BookId.ShouldBe(BookA);
        item.Title.ShouldBe("The Title");
        item.Author.ShouldBe("The Author");
        item.Description.ShouldBe("The Description");
        item.KeywordRank.ShouldBe(1);
        item.SemanticRank.ShouldBeNull();
    }

    /// <summary>
    /// Creates a list of <see cref="SearchResultItem"/> from a sequence of (id, title) pairs.
    /// </summary>
    private static List<SearchResultItem> CreateResults(params (Guid Id, string Title)[] items)
    {
        return items
            .Select((x, i) => new SearchResultItem(x.Id, x.Title, "Author", "Description", 1.0 - i * 0.1))
            .ToList();
    }
}
