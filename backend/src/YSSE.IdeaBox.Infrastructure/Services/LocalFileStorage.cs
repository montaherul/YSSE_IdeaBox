using Microsoft.Extensions.Configuration;
using YSSE.IdeaBox.Application.Abstractions;

namespace YSSE.IdeaBox.Infrastructure.Services;

/// <summary>
/// Private local-disk file storage. Files are written under a configured root with
/// GUID-based names; path traversal and executable extensions are rejected.
/// </summary>
public sealed class LocalFileStorage : IFileStorage
{
    private static readonly HashSet<string> BlockedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".bat", ".cmd", ".ps1", ".sh", ".js", ".msi", ".com", ".scr", ".jar"
    };

    private readonly string _root;

    public LocalFileStorage(IConfiguration configuration)
    {
        var configuredRoot = configuration["Storage:Root"];
        var root = string.IsNullOrWhiteSpace(configuredRoot)
            ? Path.Combine("storage", "uploads")
            : configuredRoot;

        _root = Path.GetFullPath(Path.IsPathRooted(root)
            ? root
            : Path.Combine(Directory.GetCurrentDirectory(), root));

        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string contentType, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(content);

        var relativePath = $"{Guid.NewGuid():N}{SafeExtension(fileName)}";
        var fullPath = ResolveFullPath(relativePath);

        await using (var target = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        {
            await content.CopyToAsync(target, ct);
        }

        return relativePath;
    }

    public Task<Stream?> OpenReadAsync(string storagePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            return Task.FromResult<Stream?>(null);
        }

        var fullPath = ResolveFullPath(storagePath);
        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(storagePath))
        {
            return Task.CompletedTask;
        }

        var fullPath = ResolveFullPath(storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string ResolveFullPath(string storagePath)
    {
        var relative = storagePath.Replace('\\', '/').TrimStart('/');

        if (relative.Contains("..", StringComparison.Ordinal)
            || relative.Contains(':', StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Invalid storage path.");
        }

        var fullPath = Path.GetFullPath(Path.Combine(_root, relative));
        if (!fullPath.StartsWith(_root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Invalid storage path.");
        }

        return fullPath;
    }

    private static string SafeExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || extension.Length > 10)
        {
            return string.Empty;
        }

        foreach (var ch in extension)
        {
            if (!char.IsLetterOrDigit(ch) && ch != '.')
            {
                return string.Empty;
            }
        }

        return BlockedExtensions.Contains(extension) ? string.Empty : extension.ToLowerInvariant();
    }
}
