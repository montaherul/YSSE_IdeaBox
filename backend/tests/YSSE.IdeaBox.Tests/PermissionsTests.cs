using YSSE.IdeaBox.Application.Common;

namespace YSSE.IdeaBox.Tests;

/// <summary>The permission catalog is the source of truth for dynamic authorization policies.</summary>
public class PermissionsTests
{
    [Fact]
    public void IsDefined_recognizes_catalog_codes() =>
        Assert.True(Permissions.IsDefined(Permissions.UsersRead));

    [Fact]
    public void IsDefined_is_case_insensitive() =>
        Assert.True(Permissions.IsDefined(Permissions.UsersRead.ToUpperInvariant()));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not.a.permission")]
    [InlineData(null)]
    public void IsDefined_rejects_unknown_or_empty(string? code) =>
        Assert.False(Permissions.IsDefined(code));

    [Fact]
    public void Catalog_contains_no_duplicate_codes()
    {
        var duplicates = Permissions.All
            .GroupBy(code => code, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        Assert.Empty(duplicates);
    }
}
