using YSSE.IdeaBox.Application.Common;

namespace YSSE.IdeaBox.Tests;

/// <summary>Client pagination normalization (NFR-03): default page 1, default size 20, cap 100.</summary>
public class PaginationTests
{
    [Fact]
    public void Normalize_uses_defaults_when_both_null()
        => Assert.Equal((1, Pagination.DefaultPageSize), Pagination.Normalize(null, null));

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-3, -5)]
    public void Normalize_clamps_non_positive_input(int page, int pageSize)
    {
        var (p, s) = Pagination.Normalize(page, pageSize);
        Assert.Equal(1, p);
        Assert.Equal(Pagination.DefaultPageSize, s);
    }

    [Theory]
    [InlineData(2, 50, 2, 50)]
    [InlineData(5, 200, 5, Pagination.MaxPageSize)]
    [InlineData(1, 1, 1, 1)]
    public void Normalize_preserves_valid_input_beyond_positive_floor(int page, int pageSize, int expPage, int expSize)
        => Assert.Equal((expPage, expSize), Pagination.Normalize(page, pageSize));

    [Theory]
    [InlineData(1, null, 1, Pagination.DefaultPageSize)]
    [InlineData(null, 7, 1, 7)]
    public void Normalize_defaults_only_missing_dimension(int? page, int? pageSize, int expPage, int expSize)
        => Assert.Equal((expPage, expSize), Pagination.Normalize(page, pageSize));
}

/// <summary>Server-side paging envelope math used by list endpoints.</summary>
public class PagedResultTests
{
    [Theory]
    [InlineData(100, 20, 5, true, false)]     // exact multiple, page 1 of 5: next, never previous
    [InlineData(101, 20, 6, true, false)]     // rounded up: one more page
    [InlineData(0, 20, 0, false, false)]      // empty result
    [InlineData(7, 20, 1, false, false)]      // single partial page
    public void Envelope_math_is_correct(int total, int size, int expPages, bool expNext, bool expPrev)
    {
        var p = PagedResult<int>.Create(Array.Empty<int>(), page: 1, pageSize: size, total: total);
        Assert.Equal(expPages, p.TotalPages);
        Assert.Equal(expNext, p.HasNext);
        Assert.Equal(expPrev, p.HasPrevious);
    }
}