using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1/settings")]
[Authorize]
public sealed class SettingsController : ApiControllerBase
{
    private readonly IPlatformSettingsService _settings;

    public SettingsController(IPlatformSettingsService settings) => _settings = settings;

    [HttpGet]
    [Authorize(Policy = Permissions.SettingsRead)]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        var settings = await _settings.GetSettingsAsync(ct);
        var map = settings.ToDictionary(s => s.Key, s => s.Value);
        return OkData(map);
    }

    [HttpPut]
    [Authorize(Policy = Permissions.SettingsManage)]
    public async Task<IActionResult> UpdateSettings([FromBody] Dictionary<string, JsonElement> values, CancellationToken ct)
    {
        var map = values.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ValueKind == JsonValueKind.Null ? string.Empty : kvp.Value.ToString());

        return OkResult(await _settings.UpdateSettingsAsync(map, ct));
    }
}