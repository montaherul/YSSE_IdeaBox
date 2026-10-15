namespace YSSE.IdeaBox.Application.Common;

/// <summary>
/// Server-side upload size ceilings. The API host enforces a global request-body
/// limit; these are the per-purpose business limits applied by services.
/// </summary>
public static class UploadLimits
{
    /// <summary>Profile picture (already downscaled/converted to WebP client-side).</summary>
    public const long MaxAvatarBytes = 5 * 1024 * 1024;

    /// <summary>Single private PDF holding the full idea description.</summary>
    public const long MaxIdeaAttachmentBytes = 15 * 1024 * 1024;
}
