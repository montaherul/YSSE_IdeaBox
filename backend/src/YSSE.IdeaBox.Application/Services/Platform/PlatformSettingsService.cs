using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;

namespace YSSE.IdeaBox.Application.Services.Platform;

public sealed class PlatformSettingsService : IPlatformSettingsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditWriter _audit;

    public PlatformSettingsService(IUnitOfWork unitOfWork, IAuditWriter audit)
    {
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<IReadOnlyList<PlatformSettingDto>> GetSettingsAsync(CancellationToken ct = default)
    {
        return await _unitOfWork.Repository<PlatformSetting>().Query()
            .OrderBy(p => p.Key)
            .Select(p => new PlatformSettingDto(p.Key, p.Value, p.Description))
            .ToListAsync(ct);
    }

    public async Task<Result> UpdateSettingsAsync(IReadOnlyDictionary<string, string> values, CancellationToken ct = default)
    {
        if (values.Count == 0)
        {
            return Result.Ok("Settings updated.");
        }

        var keys = values.Keys.ToList();
        var repository = _unitOfWork.Repository<PlatformSetting>();
        var existing = await repository.Query().Where(p => keys.Contains(p.Key)).ToListAsync(ct);
        var byKey = existing.ToDictionary(p => p.Key);

        foreach (var (key, value) in values)
        {
            if (byKey.TryGetValue(key, out var setting))
            {
                setting.Value = value;
            }
            else
            {
                await repository.AddAsync(new PlatformSetting { Key = key, Value = value }, ct);
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("PlatformSettingsUpdated", nameof(PlatformSetting),
            null, null, new { keys }, ct);
        return Result.Ok("Settings updated.");
    }
}
