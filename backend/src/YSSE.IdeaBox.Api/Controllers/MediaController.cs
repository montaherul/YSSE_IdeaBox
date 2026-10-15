using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions;

namespace YSSE.IdeaBox.Api.Controllers;

/// <summary>
/// Serves stored image media (profile pictures) by opaque GUID file name. Names are
/// unguessable, so the URL acts as a capability. Only image extensions are served here;
/// private documents such as idea PDFs are streamed through their authorized endpoints.
/// </summary>
[ApiController]
[Route("api/v1/media")]
[AllowAnonymous]
public sealed class MediaController : ControllerBase
{
    private static readonly Dictionary<string, string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".webp"] = "image/webp",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg"
    };

    private readonly IFileStorage _files;

    public MediaController(IFileStorage files) => _files = files;

    [HttpGet("{fileName}")]
    public async Task<IActionResult> Get(string fileName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(fileName)
            || fileName.Contains('/') || fileName.Contains('\\') || fileName.Contains("..", StringComparison.Ordinal))
        {
            return NotFound();
        }

        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrEmpty(extension) || !AllowedContentTypes.TryGetValue(extension, out var contentType))
        {
            return NotFound();
        }

        var stream = await _files.OpenReadAsync(fileName, ct);
        if (stream is null)
        {
            return NotFound();
        }

        // GUID names are stable per stored file; replacing an avatar produces a new name.
        Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        return File(stream, contentType);
    }
}
