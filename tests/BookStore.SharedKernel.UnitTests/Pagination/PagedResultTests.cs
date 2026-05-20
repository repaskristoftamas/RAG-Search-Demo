using BookStore.SharedKernel.Pagination;
using Shouldly;

namespace BookStore.SharedKernel.UnitTests.Pagination;

/// <summary>
/// Tests for <see cref="PagedResult{T}"/> pagination calculations.
/// </summary>
public sealed class PagedResultTests
{
    [Fact]
    public void TotalPages_ExactDivision_ReturnsCorrectCount()
    {
        var result = new PagedResult<string>(["a", "b"], TotalCount: 10, Page: 1, PageSize: 5);

        result.TotalPages.ShouldBe(2);
    }

    [Fact]
    public void TotalPages_PartialLastPage_RoundsUp()
    {
        var result = new PagedResult<string>(["a"], TotalCount: 11, Page: 1, PageSize: 5);

        result.TotalPages.ShouldBe(3);
    }

    [Fact]
    public void TotalPages_ZeroTotalCount_ReturnsZero()
    {
        var result = new PagedResult<string>([], TotalCount: 0, Page: 1, PageSize: 10);

        result.TotalPages.ShouldBe(0);
    }

    [Fact]
    public void TotalPages_ZeroPageSize_ReturnsZero()
    {
        var result = new PagedResult<string>([], TotalCount: 10, Page: 1, PageSize: 0);

        result.TotalPages.ShouldBe(0);
    }

    [Fact]
    public void TotalPages_NegativePageSize_ReturnsZero()
    {
        var result = new PagedResult<string>([], TotalCount: 10, Page: 1, PageSize: -1);

        result.TotalPages.ShouldBe(0);
    }

    [Fact]
    public void HasNextPage_OnFirstOfManyPages_ReturnsTrue()
    {
        var result = new PagedResult<string>(["a"], TotalCount: 20, Page: 1, PageSize: 10);

        result.HasNextPage.ShouldBeTrue();
    }

    [Fact]
    public void HasNextPage_OnLastPage_ReturnsFalse()
    {
        var result = new PagedResult<string>(["a"], TotalCount: 20, Page: 2, PageSize: 10);

        result.HasNextPage.ShouldBeFalse();
    }

    [Fact]
    public void HasNextPage_SinglePage_ReturnsFalse()
    {
        var result = new PagedResult<string>(["a"], TotalCount: 3, Page: 1, PageSize: 10);

        result.HasNextPage.ShouldBeFalse();
    }

    [Fact]
    public void HasPreviousPage_OnFirstPage_ReturnsFalse()
    {
        var result = new PagedResult<string>(["a"], TotalCount: 20, Page: 1, PageSize: 10);

        result.HasPreviousPage.ShouldBeFalse();
    }

    [Fact]
    public void HasPreviousPage_OnSecondPage_ReturnsTrue()
    {
        var result = new PagedResult<string>(["a"], TotalCount: 20, Page: 2, PageSize: 10);

        result.HasPreviousPage.ShouldBeTrue();
    }

    [Fact]
    public void TotalPages_SingleItem_ReturnsOne()
    {
        var result = new PagedResult<string>(["a"], TotalCount: 1, Page: 1, PageSize: 10);

        result.TotalPages.ShouldBe(1);
    }
}
