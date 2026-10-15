namespace YSSE.IdeaBox.Application.Common;

/// <summary>
/// Relative route under which stored media (profile pictures) is served. Files are named
/// with unguessable GUIDs, so the URL doubles as an opaque capability. Only image
/// extensions are exposed; private documents are served through authorized endpoints.
/// </summary>
public static class MediaPaths
{
    public const string Route = "/api/v1/media";

    public static string Url(string storagePath) => $"{Route}/{storagePath}";

    /// <summary>Extracts the stored file name from a media URL, or null when it is external/absent.</summary>
    public static string? StoragePathFromUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var prefix = Route + "/";
        return url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? url[prefix.Length..]
            : null;
    }
}
