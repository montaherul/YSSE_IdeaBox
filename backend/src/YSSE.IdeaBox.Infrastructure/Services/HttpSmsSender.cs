using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using YSSE.IdeaBox.Application.Abstractions;

namespace YSSE.IdeaBox.Infrastructure.Services;

/// <summary>
/// Generic HTTP SMS gateway configured from the <c>Sms</c> configuration section.
/// Sends a JSON payload (<c>{ from, to, body }</c>) to a provider endpoint. When SMS
/// is not enabled/configured it degrades to a logged no-op so local development and
/// CI never depend on an SMS provider. Message bodies and API keys are never logged.
/// </summary>
public sealed class HttpSmsSender : ISmsSender
{
    private const string HttpClientName = "sms-gateway";

    private readonly SmsOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpSmsSender> _logger;

    public HttpSmsSender(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<HttpSmsSender> logger)
    {
        _options = SmsOptions.Load(configuration);
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task SendAsync(SmsMessage message, CancellationToken ct = default)
    {
        if (!_options.IsConfigured)
        {
            _logger.LogInformation(
                "SMS delivery is not configured; skipped sending to {Recipient}.",
                MaskRecipient(message.To));
            return;
        }

        var client = _httpClientFactory.CreateClient(HttpClientName);
        using var request = new HttpRequestMessage(HttpMethod.Post, _options.EndpointUrl)
        {
            Content = JsonContent.Create(new
            {
                from = _options.From,
                to = message.To,
                body = message.Body
            })
        };

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            request.Headers.TryAddWithoutValidation(_options.ApiKeyHeader, _options.ApiKey);
        }

        using var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "SMS gateway returned {StatusCode} while sending to {Recipient}.",
                (int)response.StatusCode, MaskRecipient(message.To));
        }
    }

    private static string MaskRecipient(string recipient)
    {
        if (string.IsNullOrEmpty(recipient))
        {
            return "***";
        }

        var visible = recipient.Length <= 4 ? recipient[..1] : recipient[..3];
        return $"{visible}***{recipient[^2..]}";
    }

    private sealed record SmsOptions(
        bool Enabled, string? EndpointUrl, string? ApiKey, string ApiKeyHeader, string? From)
    {
        public bool IsConfigured =>
            Enabled && !string.IsNullOrWhiteSpace(EndpointUrl) && !string.IsNullOrWhiteSpace(From);

        public static SmsOptions Load(IConfiguration configuration)
        {
            var section = configuration.GetSection("Sms");
            return new SmsOptions(
                string.Equals(section["Enabled"], "true", StringComparison.OrdinalIgnoreCase),
                section["EndpointUrl"],
                section["ApiKey"],
                string.IsNullOrWhiteSpace(section["ApiKeyHeader"]) ? "Authorization" : section["ApiKeyHeader"]!,
                section["From"]);
        }
    }
}
